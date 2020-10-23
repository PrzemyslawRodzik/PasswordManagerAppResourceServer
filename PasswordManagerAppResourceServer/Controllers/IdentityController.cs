using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using AutoMapper;
using EmailService;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using PasswordManagerAppResourceServer.CustomExceptions;
using PasswordManagerAppResourceServer.Dtos;
using PasswordManagerAppResourceServer.Interfaces;
using PasswordManagerAppResourceServer.Models;
using PasswordManagerAppResourceServer.Responses;
using PasswordManagerAppResourceServer.Results;
using PasswordManagerAppResourceServer.Routes;
using PasswordManagerAppResourceServer.Services;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PasswordManagerAppResourceServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {

        
        private readonly IUserService _userService;
        private readonly IEmailSender _emailSender;

        public IdentityController(IUserService userService, IEmailSender emailSender)
        {

            
            _userService = userService;
            _emailSender = emailSender;
        }


        // POST api/identity/authenticate
        [HttpPost]
        [Route("login")]
        public IActionResult LogIn([FromBody] UserLoginRequest model)
        {
            
            var authUser = _userService.Authenticate(model.Email, model.Password);
            if(authUser is null)
                throw new AuthenticationException("Incorrect email or password!");
            
            if (authUser.TwoFactorAuthorization == 1)
                {
                    _userService.SendTotpToken(authUser);
                    return Ok(new
                    {
                        Success = true,
                        TwoFactorLogIn = true,
                        UserId = authUser.Id
                    });
                }


            return Ok(new AuthResponse
            {
                 Success = true,
                 AccessToken = _userService.GenerateAuthToken(authUser)
            });
               
        }


        // POST api/identity/register
        [HttpPost("register")]
        public  IActionResult Register([FromBody] UserRegistrationRequest request)
        {
            User newUser;
            
            newUser = _userService.Create(request.Email, request.Password); 
            
            return Ok(new AuthResponse
            {
                Success = true,
                AccessToken = _userService.GenerateAuthToken(newUser)
            });

        }
    
    [HttpPost("twofactorlogin")]
    public IActionResult TwoFactorLogIn([FromBody] TwoFactorLoginRequest model)
        {
            var user = _userService.GetById(model.UserId);
            var verificationStatus = _userService.VerifyTotpToken(user, model.Token);
            if (verificationStatus != 1)
            {
                if (verificationStatus == 0)
                    return BadRequest(new 
                        {
                            Success = false,
                            VerificationStatus = 0,
                            Messages = new string[] {"Wrong code"}
                        });
                else
                    return BadRequest(new
                        {
                            Success = false,
                            VerificationStatus = 2,
                            Messages = new string[] { "Code expired" }
                        });
            }
            else
                return Ok(new
                {
                    Success = true,
                    VerificationStatus = 1,
                    AccessToken = _userService.GenerateAuthToken(user)
                });
        }




        [HttpPost("twofactorlogin/resendtotp")]
    public IActionResult ResendTotp([FromBody]int idUser)
    {
            try
            {
                var user = _userService.GetById(idUser);
                _userService.SendTotpToken(user);
            }
            catch(Exception)
            {
                throw new UserServiceException("There was an error sending email. Try again later or contact support.");
            }
            
            return Ok(new ApiResponse
            {
                Success = true,
                
            });
            
    }

}




}



    

