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
using PasswordManagerAppResourceServer.Responses;

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
        [FromQuery]int?compromised,[FromQuery]int?expired)
        {
            List<LoginData> logins = null;
            if (userId is null && compromised is null)
                logins = _unitOfWork.Context.LoginDatas.ToList();
            else if (userId != null && expired == 1)
                logins = _unitOfWork.Wallet.GetUnchangedPasswordsForUser(userId.Value).ToList();
            else if (userId != null && compromised is null)
                logins = _unitOfWork.Context.LoginDatas.
                Where(x => x.UserId == userId).ToList();
            else if (userId != null && compromised == 1)
                logins = _unitOfWork.Context.LoginDatas.
                Where(x => x.UserId == userId && x.Compromised == 1).ToList();
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
        [AllowAnonymous]
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

        [AllowAnonymous]
        [HttpPost("share")]
        public ActionResult ShareLoginData([FromBody]ShareLoginModel model)
        { 
           var receiver = _unitOfWork.Context.Users.FirstOrDefault(x=>x.Email.Equals(model.ReceiverEmail));
            if(receiver is null)
                return Ok(new ApiResponse{
                    Success = false,
                    Messages = new string[]{"User with this email does not exist!"}
                });
            _unitOfWork.Context.LoginDatas.Add(model.LoginData);
            _unitOfWork.SaveChanges();
            var sharedLoginData = _unitOfWork.Context.LoginDatas.FirstOrDefault(x=>x.Name.Equals(model.LoginData.Name));
            _unitOfWork.Context.SharedLoginsData.Add(new SharedLoginData{
                LoginDataId = sharedLoginData.Id,
                UserId = receiver.Id,
                StartDate = model.StartDate,
                EndDate= model.EndDate

            });
            _unitOfWork.SaveChanges();
            return  Ok(new ApiResponse
            {
                Success = true,
                Messages = new string[] {$"This login data is now available for {receiver.Email}"}
            });



            
        }
        [AllowAnonymous]
        [HttpGet("share")]
        public ActionResult GetSharedLoginData([FromQuery] int? userId)
        { 
            if(userId is null)
            return NoContent();
            var date = DateTime.UtcNow.ToLocalTime();
           var sharedLoginTable = _unitOfWork.Context.SharedLoginsData.Where(x=>x.UserId==userId && x.EndDate>date);
           if(sharedLoginTable is null)
                return NoContent();
            List<SharedLoginModel> sharedLogins = new List<SharedLoginModel>();
            foreach(var record in sharedLoginTable)
            {
                var loginDto = _mapper.Map<LoginDataDto>(_unitOfWork.Context.LoginDatas.FirstOrDefault(a=>a.Id==record.LoginDataId));
                sharedLogins.Add(new SharedLoginModel{
                    LoginData = loginDto,
                    StartDate = record.StartDate,
                    EndDate = record.EndDate
                });

                

            }
            return Ok(sharedLogins);
        }

        




    }
}
