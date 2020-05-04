using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        public MessagesController(IDatingRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Ok(new
                {
                    success = CommonConstant.Failure,
                    message = CommonConstant.unAuthorizedUser
                });

            var messageFromRepo = await _repo.GetMessage(id);

            if (messageFromRepo == null)
            {
                return Ok(new
                {
                    success = CommonConstant.Failure,
                    message = CommonConstant.noMessage
                });
            }

            return Ok(new
            {
                success = CommonConstant.Success,
                message = messageFromRepo
            });
        }
        
        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(int userId, 
            [FromQuery]MessageParams messageParams)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Ok(new
                {
                    success = CommonConstant.Failure,
                    message = CommonConstant.unAuthorizedUser
                });
            }

            messageParams.UserId = userId;

            var messagesFromRepo = await _repo.GetMessagesForUser(messageParams);

            var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

            Response.AddPagination(messagesFromRepo.CurrentPage, messagesFromRepo.PageSize, 
                messagesFromRepo.TotalCount, messagesFromRepo.TotalPages);

            return Ok(new
            {
                success = CommonConstant.Success,
                message = messages.OrderBy(m => m.MessageSent)
            });
        }

        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread(int userId, int recipientId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Ok(new
                {
                    success = CommonConstant.Success,
                    message = CommonConstant.unAuthorizedUser
                });

            var messagesFromRepo = await _repo.GetMessageThread(userId, recipientId);

            var messageThread = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

            return Ok(new
            {
                success = CommonConstant.Success,
                messages = messageThread.OrderBy(m => m.MessageSent)
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreationDto)
        {
            var sender = await _repo.GetUser(userId);

            if (sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Ok(new
                {
                    success = CommonConstant.Failure,
                    message = CommonConstant.unAuthorizedUser
                });
            }

            messageForCreationDto.SenderId = userId;

            var recipient = await _repo.GetUser(messageForCreationDto.RecipientId);

            if (recipient == null)
            {
                return Ok(new
                {
                    success = string.Empty,
                    message = CommonConstant.unAuthorizedUser
                });
            }

            var message = _mapper.Map<Message>(messageForCreationDto);

            _repo.Add(message);

            if (await _repo.SaveAll())
            {
                var messageToReturn = _mapper.Map<MessageToReturnDto>(message);
                // return CreatedAtRoute("GetMessage", new {id = message.Id}, messageToReturn);
                return Ok(new
                {
                    success = CommonConstant.Success,
                    message = messageToReturn
                });

            }

            throw new Exception("Creating the message failed on save");
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int id, int userId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Ok(new
                {
                    muccess = string.Empty,
                    message = CommonConstant.unAuthorizedUser
                });
            }

            var messageFromRepo = await _repo.GetMessage(id);

            if (messageFromRepo.SenderId == userId)
                messageFromRepo.SenderDeleted = true;

            if (messageFromRepo.RecipientId == userId)
                messageFromRepo.RecipientDeleted = true;

            if (messageFromRepo.SenderDeleted && messageFromRepo.RecipientDeleted)
                _repo.Delete(messageFromRepo);
            
            if (await _repo.SaveAll())
            {
                return Ok(new
                {
                    success = CommonConstant.Success,
                    message = CommonConstant.msgDeleted
                });
            }
            else
            {
                return Ok(new
                {
                    success = CommonConstant.Failure,
                    message = CommonConstant.msgDeleteFail
                });
            }
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Ok(new
                {
                    success = string.Empty,
                    message = CommonConstant.unAuthorizedUser
                });

            var message = await _repo.GetMessage(id);

            if (message.RecipientId != userId)
                return Ok(new
                {
                    success = string.Empty,
                    message = CommonConstant.unAuthorizedUser
                });

            message.IsRead = true;
            message.DateRead = DateTime.Now;

            await _repo.SaveAll();

            return Ok(new
            {
                success = CommonConstant.Success
            });
        }
    }
}