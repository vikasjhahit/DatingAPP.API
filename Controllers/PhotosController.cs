using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Xml.Schema;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repo, IMapper mapper,
            IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _cloudinaryConfig = cloudinaryConfig;
            _mapper = mapper;
            _repo = repo;

            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            try
            {
                var photoFromRepo = await _repo.GetPhoto(id);

                var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);

                return Ok(new
                {
                    status = CommonConstant.Success,
                    photo = photo
                }) ;
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = CommonConstant.Failure
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm]PhotoForCreationDto photoForCreationDto)
        {
            try
            {
                if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                    return Ok(new
                    {
                        Success = string.Empty,
                        Message = CommonConstant.unAuthorizedUser
                    });

                var userFromRepo = await _repo.GetUser(userId);

                var file = photoForCreationDto.File;

                var uploadResult = new ImageUploadResult();

                if (file.Length > 0)
                {
                    using (var stream = file.OpenReadStream())
                    {
                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(file.Name, stream),
                            Transformation = new Transformation()
                                .Width(500).Height(500).Crop("fill").Gravity("face")
                        };

                        uploadResult = _cloudinary.Upload(uploadParams);

                        photoForCreationDto.Url = uploadResult.Uri.ToString();
                        photoForCreationDto.PublicId = uploadResult.PublicId;

                        var photo = _mapper.Map<Photo>(photoForCreationDto);

                        if (!userFromRepo.Photos.Any(u => u.IsMain))
                            photo.IsMain = true;

                        userFromRepo.Photos.Add(photo);

                        if (await _repo.SaveAll())
                        {
                            var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
                            //  return CreatedAtRoute("GetPhoto", new { id = photo.Id }, photoToReturn);

                            return Ok(new
                            {
                                status = CommonConstant.Success,
                                message = CommonConstant.uploadPhotoSuccess,
                                photoToReturn = photoToReturn
                            });
                        }
                        else
                        {
                            return Ok(new
                            {
                                status = CommonConstant.Failure,
                                message = CommonConstant.uploadPhotoFail
                            });
                        }
                    }
                }
                else
                {
                    return Ok(new
                    {
                        status = CommonConstant.Failure,
                        message = CommonConstant.uploadPhotoFormatError
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = CommonConstant.Failure,
                    message = CommonConstant.uploadPhotoFail
                });
            }
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            try
            {
                if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                {
                    return Ok(new
                    {
                        status = CommonConstant.Failure,
                        message = CommonConstant.unAuthorizedUser
                    });
                }

                var user = await _repo.GetUser(userId);

                if (id == 0)
                {
                    if (user.Photos.Count > 0)
                    {
                        var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);

                        currentMainPhoto.IsMain = false;
                        //var mainPhoto = user.Photos.Where(p => p.IsMain = true).FirstOrDefault();
                        //mainPhoto.IsMain = false;
                        if (await _repo.SaveAll())
                        {
                            return Ok(new
                            {
                                status = CommonConstant.Success,
                                message = CommonConstant.setMainPhotoSuccess
                            });
                        }
                        else
                        {
                            return Ok(new
                            {
                                status = CommonConstant.Failure,
                                message = CommonConstant.setMainPhotoFail
                            });
                        }
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = CommonConstant.Failure,
                            message = CommonConstant.setMainPhotoFail
                        });
                    }
                }
                else
                {

                    if (!user.Photos.Any(p => p.Id == id))
                    {
                        return Ok(new
                        {
                            status = CommonConstant.Failure,
                            message = CommonConstant.unAuthorizedUser
                        });
                    }

                    var photoFromRepo = await _repo.GetPhoto(id);

                    if (photoFromRepo.IsMain)
                    {
                        return Ok(new
                        {
                            status = CommonConstant.Failure,
                            message = CommonConstant.AlreadyMainPhoto
                        });
                    }

                    var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);
                    if (currentMainPhoto == null)
                    {
                        photoFromRepo.IsMain = true;
                    }
                    else
                    {
                        currentMainPhoto.IsMain = false;
                        photoFromRepo.IsMain = true;
                    }

                    if (await _repo.SaveAll())
                    {
                        return Ok(new
                        {
                            status = CommonConstant.Success,
                            message = CommonConstant.setMainPhotoSuccess
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = CommonConstant.Failure,
                            message = CommonConstant.setMainPhotoFail
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = CommonConstant.Failure,
                    message = CommonConstant.setMainPhotoFail
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            try
            {
                if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                    return Ok(new
                    {
                        Success = string.Empty,
                        Message = CommonConstant.unAuthorizedUser
                    });

                var user = await _repo.GetUser(userId);

                if (!user.Photos.Any(p => p.Id == id))
                    return Ok(new
                    {
                        Success = string.Empty,
                        Message = CommonConstant.unAuthorizedUser
                    });

                var photoFromRepo = await _repo.GetPhoto(id);

                if (photoFromRepo.IsMain)
                {
                    return Ok(new
                    {
                        status = CommonConstant.Failure,
                        message = CommonConstant.mainPhotoDeleteMsg
                    });
                }

                if (photoFromRepo.PublicId != null)
                {
                    var deleteParams = new DeletionParams(photoFromRepo.PublicId);
                    var result = _cloudinary.Destroy(deleteParams);

                    if (result.Result == "ok")
                    {
                        _repo.Delete(photoFromRepo);
                    }
                }

                if (photoFromRepo.PublicId == null)
                {
                    _repo.Delete(photoFromRepo);
                }

                if (await _repo.SaveAll())
                {
                    return Ok(new
                    {
                        status = CommonConstant.Success,
                        message = CommonConstant.photoDeletedSuccess
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = CommonConstant.Failure,
                        message = CommonConstant.photoDeletedFail
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = CommonConstant.Failure,
                    message = CommonConstant.photoDeletedFail
                });
            }
        }
    }
}