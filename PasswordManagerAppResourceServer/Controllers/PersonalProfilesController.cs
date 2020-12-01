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

namespace PasswordManagerAppResourceServer.Controllers
{
    [Authorize]
    [Route("api/")]
    [ApiController]
    public class PersonalProfilesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;


        public PersonalProfilesController(IMapper mapper, IUnitOfWork unitOfWork)
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

        // GET: api/personalinfos
        [HttpGet("personalinfos")]
        public ActionResult<IEnumerable<PersonalInfoDto>> GetAllProfiles(int? userId)
        {
            List<PersonalInfo> profiles = null;

            if (userId is null)
                profiles = _unitOfWork.Context.PersonalInfos.ToList();
            else if (userId != null)
                profiles = _unitOfWork.Context.PersonalInfos.Where(x => x.UserId == userId).ToList();
            if (profiles.Count <= 0)
                return NoContent();
             var profilesDto = _mapper.Map<IEnumerable<PersonalInfoDto>>(profiles);
            return Ok(profilesDto);


        }

        // GET api/personalinfos/5
        [HttpGet("personalinfos/{id}", Name = "GetProfileById")]
        public ActionResult<PersonalInfoDto> GetProfileById(int id)
        {

            var profile = _unitOfWork.Context.PersonalInfos.Find(id);
            var profileDto = _mapper.Map<PersonalInfoDto>(profile);
            if (profile != null)
                return Ok(profileDto);

            return NotFound();


        }

        // POST api/personalinfos
        [HttpPost("personalinfos")]
        public ActionResult<PersonalInfoDto> CreateProfile([FromBody] PersonalInfoDto profileDto)
        {
            var profile = _mapper.Map<PersonalInfo>(profileDto);
            profile.User = _unitOfWork.Users.Find<User>(GetUserIdFromJwtToken());

            _unitOfWork.Context.PersonalInfos.Add(profile);
            _unitOfWork.SaveChanges();

            var profileDtoResult = _mapper.Map<PersonalInfoDto>(profile);

            return CreatedAtRoute(nameof(GetProfileById), new { Id = profile.Id }, profileDtoResult);

        }

        // PUT api/personalinfos/5
        [HttpPut("personalinfos/{id}")]
        public ActionResult UpdateProfile(int id, [FromBody] PersonalInfoDto profileDto)
        {
            var profileFromDb = _unitOfWork.Context.PersonalInfos.Find(id);
            if (profileFromDb is null)
                return NotFound();

            _mapper.Map(profileDto, profileFromDb);
            _unitOfWork.Context.PersonalInfos.Update(profileFromDb);
            _unitOfWork.SaveChanges();

            return NoContent();
        }

        // DELETE api/personalinfos/5
        [HttpDelete("personalinfos/{id}")]
        public ActionResult DeleteProfile(int id)
        {

            var profile = _unitOfWork.Context.PersonalInfos.Find(id);
            if (profile.UserId != GetUserIdFromJwtToken())
                return BadRequest();
            if (profile is null)
                return NotFound();
            _unitOfWork.Context.PersonalInfos.Remove(profile);
            _unitOfWork.SaveChanges();
            return NoContent();
        }







    }
}