using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
        {
            _mapper = mapper;
            _config = config;
            _repo = repo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            try
            {
                userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

                if (await _repo.UserExists(userForRegisterDto.Username))
                    return Ok(CommonConstant.userAlreadyExist);

                var userToCreate = _mapper.Map<User>(userForRegisterDto);

                var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

                var userToReturn = _mapper.Map<UserForDetailedDto>(createdUser);

                //  return CreatedAtRoute("GetUser", new { controller = "Users", id = createdUser.Id }, userToReturn);
                return Ok(new
                {
                    status = CommonConstant.Success,
                    message = CommonConstant.userRegistrationSuccess,
                    user = userToReturn
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = CommonConstant.Failure,
                    message = CommonConstant.userRegistrationFail
                });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            try
            {
                var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

                if (userFromRepo == null)
                {
                    return Ok(new
                    {
                        status = CommonConstant.Failure,
                        token = string.Empty,
                        message = CommonConstant.invalidUserNamePassword
                    });
                }
                else
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                        new Claim(ClaimTypes.Name, userFromRepo.Username)
                    };

                    var key = new SymmetricSecurityKey(Encoding.UTF8
                        .GetBytes(_config.GetSection("AppSettings:Token").Value));

                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(claims),
                        Expires = DateTime.Now.AddDays(1),
                        SigningCredentials = creds
                    };

                    var tokenHandler = new JwtSecurityTokenHandler();

                    var token = tokenHandler.CreateToken(tokenDescriptor);

                    var user = _mapper.Map<UserForListDto>(userFromRepo);

                    return Ok(new
                    {
                        status = CommonConstant.Success,
                        token = tokenHandler.WriteToken(token),
                        user = user
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = CommonConstant.Failure,
                    token = string.Empty,
                    message = CommonConstant.loginFailed
                });
            }
        }

        [HttpGet("getcountrylist")]
        public List<CountryCityList> GetCountryList()
        {

            List<string> CountryList = new List<string>();
            CultureInfo[] CInfoList = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            List<CountryCityList> lstCountryCityList = new List<CountryCityList>();

            foreach (CultureInfo CInfo in CInfoList)
            {
                RegionInfo R = new RegionInfo(CInfo.LCID);
                if (!(CountryList.Contains(R.EnglishName)))
                {
                    CountryList.Add(R.EnglishName);
                }
            }

            CountryList.Sort();

            foreach (var item in CountryList)
            {
                CountryCityList objCountryCityList = new CountryCityList();
                objCountryCityList.country = item;
                objCountryCityList.cities = new List<string>() { "Mumbai", "Delhi", "Patna" };
                lstCountryCityList.Add(objCountryCityList);
            }

            return lstCountryCityList;
        }
    }
}