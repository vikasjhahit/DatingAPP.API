using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
   // [Authorize]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsersController(IDatingRepository repo, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _repo = repo;
            _httpContextAccessor = httpContextAccessor;
        }


        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            //  var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var currentUser = _httpContextAccessor.HttpContext;
            var currentUserId = int.Parse(currentUser.User.FindFirstValue(ClaimTypes.NameIdentifier));

            var userFromRepo = await _repo.GetUser(currentUserId);

            userParams.UserId = currentUserId;

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            }

            var users = await _repo.GetUsers(userParams);

            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

            Response.AddPagination(users.CurrentPage, users.PageSize,
                users.TotalCount, users.TotalPages);

            return Ok(usersToReturn);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _repo.GetUser(id);

            var userToReturn = _mapper.Map<UserForDetailedDto>(user);

            return Ok(new
            {
                status = CommonConstant.Success,
                message = CommonConstant.userRegistrationSuccess,
                user = userToReturn
            }) ;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody]JObject json)
        {
            try
            {
                UserForUpdateDto userForUpdateDto = JsonConvert.DeserializeObject<UserForUpdateDto>(json.ToString());
                if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                {
                    return Ok(new
                    {
                        Success = string.Empty,
                        Message = CommonConstant.userDetailUpdateFail
                    });
                }
                else
                {
                    var userFromRepo = await _repo.GetUser(id);

                    _mapper.Map(userForUpdateDto, userFromRepo);

                    if (await _repo.SaveAll())
                    {
                        return Ok(new
                        {
                            Success = CommonConstant.Success,
                            Message = CommonConstant.userDetailUpdateSuccess
                        }) ;
                    }
                    else
                    {
                        return Ok(new
                        {
                            Success = string.Empty,
                            Message = CommonConstant.userDetailUpdateFail
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Success = string.Empty,
                    Message = CommonConstant.userDetailUpdateFail
                });
            }
        }

        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Ok(new
                {
                    status = CommonConstant.Failure,
                    message = CommonConstant.unAuthorizedUser
                });
            }

            var like = await _repo.GetLike(id, recipientId);

            if (like != null)
            {
                return Ok(new
                {
                    status = CommonConstant.Failure,
                    message = CommonConstant.alreadyLiked
                });
            }
            
            if (await _repo.GetUser(recipientId) == null)
            {
                return Ok(new
                {
                    status = CommonConstant.Failure,
                    message = CommonConstant.unAuthorizedUser
                });
            }

            like = new Like
            {
                LikerId = id,
                LikeeId = recipientId
            };

            _repo.Add<Like>(like);

            if (await _repo.SaveAll())
            {
                return Ok(new
                {
                    Success = CommonConstant.Success
                });
            }
            else
            {
                return Ok(new
                {
                    Success = CommonConstant.Failure
                });
            }
        }

    }
}