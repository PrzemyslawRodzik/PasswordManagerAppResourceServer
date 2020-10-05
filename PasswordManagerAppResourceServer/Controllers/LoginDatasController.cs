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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PasswordManagerAppResourceServer.Controllers
{
    // api/notes
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
            return Int32.Parse(HttpContext.User.Identity.Name);
        }

        // GET: api/logindatas
        [HttpGet]
        public ActionResult<IEnumerable<LoginData>> GetAllLoginDatas([FromQuery] int? userId)
        {
            List<LoginData> logins = null;
            if (userId is null)
                logins = _unitOfWork.Context.LoginDatas.ToList();
            else
                logins = _unitOfWork.Context.LoginDatas.Where(x => x.UserId == userId).ToList();
            if (logins.Count <= 0)
                return NoContent();


            return Ok(logins);


        }










        // GET api/logindatas/5
        [HttpGet("{id}", Name = "GetLoginDatasById")]
        public ActionResult<LoginData> GetLoginDatasById(int id)
        {

            var loginData = _unitOfWork.Context.Notes.Find(id);
            
            if (loginData != null)
                return Ok(loginData);

            return NotFound();


        }

        // POST api/logindatas
        [HttpPost]
        public ActionResult<NoteDto> CreateNote([FromBody] LoginData loginData)
        {
            var note = _mapper.Map<Note>(noteDto);
            note.User = _unitOfWork.Users.Find<User>(Int32.Parse(HttpContext.User.Identity.Name));

            _unitOfWork.Context.Notes.Add(note);
            _unitOfWork.SaveChanges();

            var noteDtoResult = _mapper.Map<NoteDto>(note);

            return CreatedAtRoute(nameof(GetNoteById), new { Id = note.Id }, noteDtoResult);

        }

        // PUT api/notes/5
        [HttpPut("{id}")]
        public ActionResult UpdateNote(int id, [FromBody] NoteDto noteDto)
        {
            var noteFromDb = _unitOfWork.Context.Notes.Find(id);
            if (noteFromDb is null)
                return NotFound();




            _mapper.Map(noteDto, noteFromDb);
            _unitOfWork.Context.Notes.Update(noteFromDb);
            _unitOfWork.SaveChanges();

            return NoContent();
        }

        // DELETE api/notes/5
        [HttpDelete("{id}")]
        public ActionResult DeleteNote(int id)
        {

            var note = _unitOfWork.Context.Notes.Find(id);
            if (note.UserId != GetUserIdFromJwtToken())
                return BadRequest();
            if (note is null)
                return NotFound();
            _unitOfWork.Context.Notes.Remove(note);
            _unitOfWork.SaveChanges();
            return NoContent();
        }
    }
}
