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

        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;

        public IdentityController(IConfiguration config, IUnitOfWork unitOfWork, IUserService userService, IMapper mapper, IEmailSender emailSender)
        {

            _config = config;
            _unitOfWork = unitOfWork;
            _userService = userService;
            _userService.EmailSendEvent += UserService_EmailSendEvent;
            _mapper = mapper;
            _emailSender = emailSender;
        }


        private void UserService_EmailSendEvent(object sender, Message e)
        {
            _emailSender.SendEmailAsync(e);
        }

       



        // POST api/identity/authenticate
        [HttpPost]
        [Route("authenticate")]
        public IActionResult AuthenticateUser([FromBody] UserLoginRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new FailedResponse
                {
                    Errors = ModelState.Values.SelectMany(x => x.Errors.Select(xx => xx.ErrorMessage))
                });
            }

            var authUser = _userService.Authenticate(model.Email, model.Password);
            

            if (authUser != null)
            {
                if (authUser.TwoFactorAuthorization == 1)
                    _userService.SendTotpToken(authUser);
                return Ok(_mapper.Map<UserDto>(authUser));
            }
                
            else
                return BadRequest(new FailedResponse
                {
                    Errors = new string[] { "Incorrect email or password!" }
                });


        }
        // POST api/identity/token
        [HttpPost]
        [Route("token")]
        public IActionResult AssignToken([FromBody] UserLoginRequest model)
        {
            var authUser = _userService.Authenticate(model.Email, model.Password);

            if (authUser != null)
                return Ok(new AuthResponse
                {
                    AccessToken = _userService.GenerateAuthToken(authUser),
                    Success = true

                });
            else
                return BadRequest(new AuthResponse
                {   Success = false,
                    Messages = new string[] { "Incorrect email or password!" }
                }) ;




        }

        // POST api/identity/register
        [HttpPost("register")]
        public  IActionResult Register([FromBody] UserRegistrationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new FailedResponse
                {
                    Errors = ModelState.Values.SelectMany(x => x.Errors.Select(xx => xx.ErrorMessage))
                });
            }
            User newUser;
            try
            { newUser = _userService.Create(request.Email, request.Password); }
            catch (AppException ex)
            {
                return BadRequest(new FailedResponse
                {
                    Errors = new string[]{ ex.Message }
                });
            }
            
            return Ok(new AuthSuccessRegisterResponse
            {
                UserDto = _mapper.Map<UserDto>(newUser),
                AccessToken = _userService.GenerateAuthToken(newUser)
            });

        }
         
    /*
        return 
        AccessToken :
            JwtToken = tokenJson,
            Expire = expirationDate
    */
    

    [HttpPost("twofactorlogin")]
    public IActionResult TwoFactorLogIn([FromBody] TwoFactorLoginRequest model)
        {
            
            var user = _userService.GetById(model.UserId);
            var verificationStatus = _userService.VerifyTotpToken(user, model.Token);
            if (verificationStatus != 1)
            {
                if (verificationStatus == 0)
                {

                    return BadRequest(new 
                    {
                        Success = false,
                        VerificationStatus = 0,
                        Errors = new string[] {"Wrong code"},

                    });

                }
                else
                {
                    return BadRequest(new
                    {
                        Success = false,
                        VerificationStatus = -1,
                        Errors = new string[] { "Code expired" }

                    }); 
                    

                }
            }
            else
            {

                return Ok(new 
                {
                    Success = true,
                    VerificationStatus = 1,
                    AccessToken = _userService.GenerateAuthToken(user)
                });



            }

        }
    [HttpPost("resendtotp")]
    public IActionResult ResendTotp([FromBody]int idUser)
        {
            try
            {
                var user = _userService.GetById(idUser);
                _userService.SendTotpToken(user);

            }
            catch(Exception)
            {
                return BadRequest(new ApiResponse {
                    Success = false,
                    Messages = new string[] { "There was an error sending email. Try again later or contact support." }
                    });

            }
            return Ok(new ApiResponse
            {
                Success = true,
                
                    });
            
        }

}




}



    

