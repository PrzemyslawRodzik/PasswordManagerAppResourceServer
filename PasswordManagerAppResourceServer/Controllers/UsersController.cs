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
using Newtonsoft.Json.Linq;

namespace PasswordManagerAppResourceServer.Controllers
{   
  //  [Authorize]
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

        [HttpGet("check")]
        public ActionResult CheckEmailAvailability([FromQuery]string email)
        {

            bool emailIsInUse = _userService.VerifyEmail(email);

            if (emailIsInUse)
                return BadRequest();
            else
                return Ok();
            

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
                    Messages = new string[] { "Password is incorrect" }
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
                    Messages = new string[] { "Verification token is invalid or expired"}
                });
            var isDeleted = _userService.DeleteUser(GetUserIdFromJwtToken());
            if(!isDeleted)
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Messages = new string[] { "There was an error during user delete.Try again or contact support." }
                });
            
            return Ok(new ApiResponse
            {
                Success = true   
            });






        }

       [HttpPost("password/change")]
       public IActionResult ChangeMasterPassword([FromBody]PasswordChangeRequest model)
        {
            if(!ModelState.IsValid)
                return BadRequest(new ApiResponse{
                        Success = false,
                        Messages = ModelState.Values.SelectMany(x => x.Errors.Select(xx => xx.ErrorMessage))
                });
            var authUser = _userService.GetById(GetUserIdFromJwtToken());
            
            if (!_userService.VerifyPasswordHash(model.Password, Convert.FromBase64String(authUser.Password), Convert.FromBase64String(authUser.PasswordSalt)))
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Messages = new string [] {"Password is incorrect."}
                });
            var isSuccess = _userService.ChangeMasterPassword(model.NewPassword, GetUserIdFromJwtToken().ToString());
            if(isSuccess)
                return Ok(new ApiResponse
                {
                    Success = true,
                    Messages = new string [] {"Password has been successfully changed."}
                });
            else
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Messages = new string[] { "There was an error. Contact support or try again later." }
                });



        }
        [HttpPost("update-preferences")]
        public IActionResult UpdatePreferences([FromBody]UpdatePreferencesWrapper upPreferences)
        {
            try
            {
                _userService.UpdatePreferences(upPreferences, GetUserIdFromJwtToken());



            }
            catch(Exception)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Messages = new string[] { "There was an error." }
                });
            }
            var user = _userService.GetById(GetUserIdFromJwtToken());
            return Ok(new AuthResponse
            {
                Success = true,
                AccessToken = _userService.GenerateAuthToken(user),
                Messages = new string[] { "Settings has been updated." }
            });


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