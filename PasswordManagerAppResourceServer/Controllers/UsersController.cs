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
using PasswordManagerAppResourceServer.CustomExceptions;
using EmailService;

namespace PasswordManagerAppResourceServer.Controllers
{   
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IEmailSender _emailSender;

        public UsersController(IMapper mapper,IUserService userService, IEmailSender emailSender)
        {
            _mapper = mapper;
            _userService = userService;
            _emailSender = emailSender;
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

            throw new UserServiceException("User does not exist.");
        }
        [AllowAnonymous]
        [HttpGet("email/check")]
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
                throw new AuthenticationException("Password is incorrect");

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
            if (!tokenIsValid)
                throw new UserServiceException("Verification token is invalid or expired");
            var isDeleted = _userService.DeleteUser(GetUserIdFromJwtToken());
            if (!isDeleted)
                throw new UserServiceException("There was an error during user delete.Try again or contact support.");
            
            return Ok(new ApiResponse
            {
                Success = true   
            });

        }

       [HttpPost("password/change")]
       public IActionResult ChangeMasterPassword([FromBody]PasswordChangeRequest model)
        {
            if (!ModelState.IsValid)
                throw new AuthenticationException(ModelState.Values.SelectMany(x => x.Errors.Select(xx => xx.ErrorMessage)).ToString());
            
            var authUser = _userService.GetById(GetUserIdFromJwtToken());

            if (!_userService.VerifyPasswordHash(model.Password, Convert.FromBase64String(authUser.Password), Convert.FromBase64String(authUser.PasswordSalt)))
                throw new AuthenticationException("Password is incorrect.");
            var isSuccess = _userService.ChangeMasterPassword(model.NewPassword, GetUserIdFromJwtToken().ToString());
            return Ok(new ApiResponse
                {
                    Success = true,
                    Messages = new string[] { "Password has been successfully changed." }
                });
        }





        [HttpPost("update/preferences")]
        public IActionResult UpdatePreferences([FromBody]UpdatePreferencesWrapper upPreferences)
        {
            
            _userService.UpdatePreferences(upPreferences, GetUserIdFromJwtToken());
            return Ok(new AuthResponse
            {
                Success = true,
                AccessToken = _userService.GenerateAuthToken(_userService.GetById(GetUserIdFromJwtToken())),
                Messages = new string[] { "Settings has been updated." }
            });


        }
        [AllowAnonymous]
        [HttpPost("devices/check-guid")]
        public IActionResult CheckUserGuidDevice([FromBody]NewDeviceLogInRequest request)
        {
           var guidMatch =  _userService.CheckUserGuidDeviceInDb(request.GuidDevice, request.UserId);
            if (guidMatch)
                return Ok();
            return BadRequest();
        }
        [AllowAnonymous]
        [HttpPost("devices/authorize-new-device")]
        public IActionResult AuthorizeNewUserDevice([FromBody] NewDeviceLogInRequest request)
        {
            var authUserId = request.UserId;
            var authUserEmail = _userService.GetById(authUserId).Email;
            var ipMatchWithPrevious = _userService.CheckPreviousUserIp(authUserId, request.IpAddress);  
            _userService.AddNewDeviceToDb(request.GuidDevice, authUserId, request.IpAddress); 
           
            if (!ipMatchWithPrevious)
              _emailSender.SendEmailAsync(
                  new Message(
                new string[] { authUserEmail },
                "Nowe urz¹dzenie " + request.OSName, "Zarejestrowano logowanie z nowego adresu ip: " + request.IpAddress + ", system : " + request.OSName + " " + request.BrowserName + " dnia " + DateTime.UtcNow.ToLocalTime().ToString("yyyy-MM-dd' 'HH:mm:ss") + "."));

            return Ok();



           
               
        }




    }
}