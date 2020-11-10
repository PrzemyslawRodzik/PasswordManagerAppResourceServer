using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using PasswordManagerAppResourceServer.Dtos;
using PasswordManagerAppResourceServer.Interfaces;
using PasswordManagerAppResourceServer.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PasswordManagerAppResourceServer.Controllers
{
    // api/logindatas
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LoginDatasController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public LoginDatasController(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }
        

        private int GetUserIdFromJwtToken()
        {
            int id = -1;
            try
            {
                id = Int32.Parse(HttpContext.User.Identity.Name);
                return id;
            }
            catch (Exception)
            {
                return id;
            }
            
        }
        // GET: api/logindatas
        [AllowAnonymous]
        [HttpGet]
        public ActionResult<IEnumerable<LoginData>> GetAllLoginDatas([FromQuery] int? userId, 
        [FromQuery]int?compromised)
        {
            List<LoginData> logins = null;
            if (userId  is null &&  compromised is null)
                logins = _unitOfWork.Context.LoginDatas.ToList();
            else if(userId!=null && compromised is null)
                logins = _unitOfWork.Context.LoginDatas.
                Where(x => x.UserId == userId).ToList();
            else if(userId != null && compromised==1)
                logins = _unitOfWork.Context.LoginDatas.
                Where(x => x.UserId == userId && x.Compromised==1).ToList();
            else if (userId != null && compromised == 0)
                logins = _unitOfWork.Context.LoginDatas.
                Where(x => x.UserId == userId && x.Compromised == 0).ToList();

            if (logins.Count <= 0)
                return NoContent();

            var loginsDto = _mapper.Map<IEnumerable<LoginDataDto>>(logins);
            return Ok(loginsDto);
        }
        // GET api/logindatas/{id}
        [HttpGet("{id}", Name = "GetLoginDataById")]
        public ActionResult<LoginDataDto> GetLoginDataById(int id)
        {

            var loginData = _unitOfWork.Context.LoginDatas.Find(id);
            var loginDto = _mapper.Map<LoginDataDto>(loginData);
            if (loginData != null)
                return Ok(loginDto);

            return NotFound();
        }
        // POST api/logindatas
        [HttpPost]
        public ActionResult<LoginDataDto> CreateLoginData([FromBody] LoginDataDto loginDataDto)
        {
            var loginData = _mapper.Map<LoginData>(loginDataDto);
            loginData.User = _unitOfWork.Users.Find<User>(GetUserIdFromJwtToken());
            loginData.ModifiedDate = DateTime.UtcNow.ToLocalTime();
            _unitOfWork.Context.LoginDatas.Add(loginData);
            _unitOfWork.SaveChanges();
            var loginDtoResult = _mapper.Map<LoginDataDto>(loginData);
            return CreatedAtRoute(nameof(GetLoginDataById), new { Id = loginData.Id }, loginDtoResult);
        }
        [HttpPut("{id}")]
        public ActionResult UpdateLoginData(int id, [FromBody] LoginDataDto loginDataDto){
            var loginDataFromDb = _unitOfWork.Context.LoginDatas.Find(id);
            if (loginDataFromDb is null)
                return NotFound();
            _mapper.Map(loginDataDto, loginDataFromDb);
            loginDataFromDb.ModifiedDate = DateTime.UtcNow.ToLocalTime();
            _unitOfWork.Context.LoginDatas.Update(loginDataFromDb);
            _unitOfWork.SaveChanges();
            return NoContent();
        }
        [HttpDelete("{id}")]
        public ActionResult DeleteLoginData(int id){
            var loginData = _unitOfWork.Context.LoginDatas.Find(id);
            if (loginData.UserId != GetUserIdFromJwtToken())
                return BadRequest();
            if (loginData is null)
                return NotFound();
            _unitOfWork.Context.LoginDatas.Remove(loginData);
            _unitOfWork.SaveChanges();
            return NoContent();
        }

        [HttpPost("checklogindup")]
        public ActionResult CheckLoginDuplicate(CheckLoginRequest model)
        { bool isLoginDuplicate = true;
            try
            {
                isLoginDuplicate = _unitOfWork.Context.LoginDatas.Any(l => l.UserId == GetUserIdFromJwtToken() && l.Website == model.Website && l.Login == model.Login);
            }
            catch (ArgumentNullException)
            {
                return BadRequest();
            }
            
            if (isLoginDuplicate)
                return BadRequest();
            return Ok();
        }
        [AllowAnonymous]
        [HttpPost("check-out-of-date")]
        public  ActionResult  CheckOutOfDate([FromBody]string[] activeUsers)
        {
             Dictionary<string, string> userMessages = new Dictionary<string, string>();
             string websitesList, message;
            
             foreach(string userId in activeUsers)
             {
                var newOutOfDateLogins = _unitOfWork.Wallet.GetOutOfDateLoginsForUser(Int32.Parse(userId));
                if (newOutOfDateLogins is null || newOutOfDateLogins.Count<=0)
                    continue;
                websitesList = "";
                newOutOfDateLogins.ForEach( x => websitesList += x.Website + ", ");
                if (websitesList.Equals(""))
                    continue;
                message = $"We are detected {newOutOfDateLogins.Count} new unchanged passwords for listed sites : {websitesList}. Please change them.";
                userMessages.Add(userId, message);
             
             }
            return Ok(userMessages);

        }
        [AllowAnonymous]
        [HttpPut("updateMany")]
        public ActionResult UpdateMany([FromBody]List<LoginData> models)
        {
            if (models.Count <= 0)
                return Ok();
            _unitOfWork.Context.UpdateRange(models);
            _unitOfWork.SaveChanges();
            return Ok();
        }

        




    }
}
