﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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
        //private readonly IUserService _userService;
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public IdentityController(IConfiguration config,IUnitOfWork unitOfWork,IUserService userService, IMapper mapper)
        {
            
            _config = config;
            _unitOfWork = unitOfWork;
            _userService = userService;
            _mapper = mapper;
        }
        [Authorize]
        // GET: api/<IdentityController>
        [HttpGet]
        [Route("dane")]
        public IEnumerable<string> Get()
        {
            var currentUser = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentEmail = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
            return new string[] { currentUser, currentEmail, _config["JwtSettings:Issuer"], _config["JwtSettings:Audience"]};
        }

        

        // POST api/<IdentityController>
        [HttpPost]
        [Route("authenticate")]
        public  IActionResult AuthenticateUser([FromBody] UserLoginRequest model)
        {   
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthFailedResponse
                {
                    Errors = ModelState.Values.SelectMany(x => x.Errors.Select(xx => xx.ErrorMessage))
                });
            }

             var authUser =  _userService.Authenticate(model.Email, model.Password);

            if(authUser!=null)
                return Ok( _mapper.Map<UserDto>(authUser)  );
            else
                return BadRequest(new AuthFailedResponse
                {
                    Errors = new string[] {"Incorrect email or password!"}
                });
            

        }
        [HttpPost]
        [Route("token")]
        public  IActionResult AssignToken([FromBody] UserLoginRequest model)
        {   
             var authUser =  _userService.Authenticate(model.Email, model.Password);

            if(authUser!=null)
                return Ok(GenerateAuthToken(authUser));
             else
                return BadRequest(new AuthFailedResponse
                {
                    Errors = new string[] {"Incorrect email or password!"}
                });        
                
           
            

        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthFailedResponse
                {
                    Errors = ModelState.Values.SelectMany(x => x.Errors.Select(xx => xx.ErrorMessage))
                });
            }

            // var authResponse = await _identityService.RegisterAsync(request.Email, request.Password);
            
            AuthenticationResult authResponse = new AuthenticationResult
            {   Token = "123123123zxczxczxczxcz",
                RefreshToken = "zxczxczxczqweqwe123123",
                Success = true,
                Errors = new string[] { "Cos poszlo nie tak!" },


            }; 

            if (!authResponse.Success)
            {
                return BadRequest(new AuthFailedResponse
                {
                    Errors = authResponse.Errors
                });
            }

            return Ok(new AuthSuccessResponse
            {
                Token = authResponse.Token,
                RefreshToken = authResponse.RefreshToken
            });
        }

        // PUT api/<IdentityController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
            
        }

        // DELETE api/<IdentityController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }



    private AccessToken GenerateAuthToken(User user)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_config["JwtSettings:SecretEncyptionKey"]);
            var symmetricSecurityKey = new SymmetricSecurityKey(keyBytes);
            
            using RSA rsa = RSA.Create();
            rsa.ImportRSAPrivateKey( // Convert the loaded key from base64 to bytes.
                source: Convert.FromBase64String(_config["JwtSettings:Asymmetric:PrivateKey"]), // Use the private key to sign tokens
                bytesRead: out int _); // Discard the out variable 

            var signingCredentials = new SigningCredentials(
                key: new RsaSecurityKey(rsa),
                algorithm: SecurityAlgorithms.RsaSha256 // Important to use RSA version of the SHA algo 
            );
            var cryptoKey = new EncryptingCredentials(symmetricSecurityKey, SecurityAlgorithms.Aes256KW, SecurityAlgorithms.Aes256CbcHmacSha512);
            var expirationDate =  DateTime.UtcNow.AddMinutes(user.AuthenticationTime);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.Name,user.Id.ToString()),
                    new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub,user.Id.ToString()),
                    new Claim(ClaimTypes.Email,user.Email),
                    new Claim("Admin", user.Admin.ToString()),
                    new Claim("TwoFactorAuth", user.TwoFactorAuthorization.ToString()),
                    new Claim("PasswordNotifications", user.PasswordNotifications.ToString()),
                    new Claim("AuthTime", user.AuthenticationTime.ToString()),



                }),
                Expires =expirationDate,
                Audience = _config["JwtSettings:Audience"],
                Issuer = _config["JwtSettings:Issuer"],
                NotBefore = DateTime.UtcNow,
                IssuedAt = DateTime.UtcNow,
                SigningCredentials = signingCredentials,
                EncryptingCredentials = cryptoKey
            };

            
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenJson = tokenHandler.WriteToken(token);
            
            return new AccessToken{
                JwtToken = tokenJson,
                Expire = expirationDate
            };
    }




    }

    
}
