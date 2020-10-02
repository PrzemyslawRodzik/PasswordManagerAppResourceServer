using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManagerAppResourceServer.Dtos;
using PasswordManagerAppResourceServer.Interfaces;
using PasswordManagerAppResourceServer.Models;
using PasswordManagerAppResourceServer.Responses;
using PasswordManagerAppResourceServer.Services;

namespace PasswordManagerAppResourceServer.Controllers
{   
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        

        public UsersController(IMapper mapper,IUnitOfWork unitOfWork,IUserService userService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
           
            _userService = userService;
        }
        private int GetUserIdFromJwtToken()
        {
            return Int32.Parse(HttpContext.User.Identity.Name);
        }

        [HttpGet("getauthuser")]
        public ActionResult<UserDto> GetAuthUser()
        {
            
            User authUser = _userService.GetById(GetUserIdFromJwtToken());
            if(authUser!=null)
                return Ok(_mapper.Map<UserDto>(authUser));
            
            return NotFound(new FailedResponse{
                Errors = new string[]{"User does not exists."}
            });
        }
        [HttpPost("deleteaccount1step")]
        public ActionResult<ApiResponse> DeleteAccount1Step(DeleteAccountRequest model)
        {
           User authUser =  _userService.GetById(GetUserIdFromJwtToken());
            var passwordIsValid = _userService.VerifyPasswordHash(model.Password, Convert.FromBase64String(authUser.Password), Convert.FromBase64String( authUser.PasswordSalt));
            if (!passwordIsValid)
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Errors = new string[] { "Password is incorrect" }
                }); 

            _userService.CreateAndSendAuthorizationToken(GetUserIdFromJwtToken(), model.Password);
            return Ok(new ApiResponse
            {
                Success = true
                
            });


        }
        [HttpPost("deleteaccount2step")]
        public ActionResult<ApiResponse> DeleteAccount2Step(DeleteAccountRequest model)
        {


            bool tokenIsValid = _userService.ValidateToken(model.Token, model.Password);
            if(!tokenIsValid)
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Errors = new string[] { "Verification token is invalid or expired"}
                });
            var isDeleted = _userService.DeleteUser(GetUserIdFromJwtToken());
            if(!isDeleted)
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Errors = new string[] { "There was an error during user delete.Try again or contact support." }
                });
            
            return Ok(new ApiResponse
            {
                Success = true   
            });






        }

        [HttpGet("{id}")]
        public ActionResult<UserDto> SendTotpToken(int id)
        {
            return null;
        }

        [HttpPost("")]
        public ActionResult<UserDto> PostUser(UserDto model)
        {
            return null;
        }

        [HttpPut("{id}")]
        public IActionResult PutUser(int id, UserDto model)
        {
            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult<UserDto> DeleteUserById(int id)
        {
            return null;
        }
    }
}