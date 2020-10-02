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
    public class NotesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        

        public NotesController(IMapper mapper,IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            
        }

        private int GetUserIdFromJwtToken()
        {
            return Int32.Parse(HttpContext.User.Identity.Name);
        }

        // GET: api/notes
        [HttpGet]
        public ActionResult<IEnumerable<NoteDto>> GetAllNotes([FromQuery]int? userId)
        {
            List<Note> notes = null;
            if(userId is null)
                notes = _unitOfWork.Context.Notes.ToList();
             else
                notes = _unitOfWork.Context.Notes.Where(x=>x.UserId==userId).ToList();
            if(notes.Count<=0)
                return NoContent();
                
            
                
            
            var notesDto = _mapper.Map<IEnumerable<NoteDto>>(notes);

            
           
            
            return Ok(notesDto);    
            
            
        }
        









        // GET api/notes/5
        [HttpGet("{id}",Name="GetNoteById")]
        public ActionResult<NoteDto> GetNoteById(int id)
        {
            
            var note  = _unitOfWork.Context.Notes.Find(id);
            var  noteDto = _mapper.Map<NoteDto>(note);
            if(note!=null)
                return Ok(noteDto);

            return NotFound();
            
            
        }

        // POST api/notes
        [HttpPost]
        public ActionResult<NoteDto> CreateNote([FromBody] NoteDto noteDto)
        {
            var note = _mapper.Map<Note>(noteDto);
            note.User = _unitOfWork.Users.Find<User>(Int32.Parse(HttpContext.User.Identity.Name));
            
            _unitOfWork.Context.Notes.Add(note);
            _unitOfWork.SaveChanges();
            
            var noteDtoResult = _mapper.Map<NoteDto>(note);
              
            return CreatedAtRoute(nameof(GetNoteById),new {Id = note.Id},noteDtoResult);
            
        }

        // PUT api/notes/5
        [HttpPut("{id}")]
        public ActionResult UpdateNote(int id, [FromBody] NoteDto noteDto)
        {
            var noteFromDb = _unitOfWork.Context.Notes.Find(id);
            if(noteFromDb is null)
                return NotFound();
            



            _mapper.Map(noteDto,noteFromDb);
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
            if(note is null)
                return NotFound();
            _unitOfWork.Context.Notes.Remove(note);
            _unitOfWork.SaveChanges();
            return NoContent();
        }
    }
}
