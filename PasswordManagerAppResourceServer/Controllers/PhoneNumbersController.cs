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
    [Route("api/[controller]")]
    [ApiController]
    public class PhoneNumbersController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;


        public PhoneNumbersController(IMapper mapper, IUnitOfWork unitOfWork)
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
        
        // GET: api/phonenumbers
        [HttpGet]
        public ActionResult<IEnumerable<PhoneNumberDto>> GetAllPhoneNumbers()
        {
            List<PhoneNumber> phoneNumbers = null;

            phoneNumbers = _unitOfWork.Context.PhoneNumbers.ToList();
           

            if (phoneNumbers.Count <= 0)
                return NoContent();

            var phoneNumbersDto = _mapper.Map<IEnumerable<PhoneNumberDto>>(phoneNumbers);
            return Ok(phoneNumbersDto);


        }

        // GET api/phonenumbers/5
        [HttpGet("{id}", Name = "GetPhoneNumberById")]
        public ActionResult<PhoneNumberDto> GetPhoneNumberById(int id)
        {

            var phoneNumber = _unitOfWork.Context.PhoneNumbers.Find(id);
            var phoneNumberDto = _mapper.Map<PhoneNumberDto>(phoneNumber);
            if (phoneNumber != null)
                return Ok(phoneNumberDto);

            return NotFound();


        }

        // POST api/phonenumbers
        [HttpPost]
        public ActionResult<PhoneNumberDto> CreatePhoneNumber([FromBody] PhoneNumberDto phoneNumberDto)
        {
            var phoneNumber = _mapper.Map<PhoneNumber>(phoneNumberDto);
            phoneNumber.PersonalInfo = _unitOfWork.Context.PersonalInfos.FirstOrDefault(x => x.Id ==phoneNumber.PersonalInfoId);
            _unitOfWork.Context.PhoneNumbers.Add(phoneNumber);
            _unitOfWork.SaveChanges();

            var phoneNumberDtoResult = _mapper.Map<PhoneNumberDto>(phoneNumber);

            return CreatedAtRoute(nameof(GetPhoneNumberById), new { Id = phoneNumber.Id }, phoneNumberDtoResult);

        }

        // PUT api/phonenumbers/5
        [HttpPut("{id}")]
        public ActionResult UpdatePhoneNumber(int id, [FromBody] PhoneNumberDto phoneNumberDto)
        {
            var phoneNumberFromDb = _unitOfWork.Context.PhoneNumbers.Find(id);
            if (phoneNumberFromDb is null)
                return NotFound();

            _mapper.Map(phoneNumberDto, phoneNumberFromDb);
            
            _unitOfWork.Context.PhoneNumbers.Update(phoneNumberFromDb);
            _unitOfWork.SaveChanges();

            return NoContent();
        }

        // DELETE api/phonenumbers/5
        [HttpDelete("{id}")]
        public ActionResult DeletePhoneNumber(int id)
        {

            var phoneNumber = _unitOfWork.Context.PhoneNumbers.Find(id);
            if (phoneNumber.PersonalInfoId != _unitOfWork.Context.PersonalInfos.FirstOrDefault(x => x.UserId == GetUserIdFromJwtToken()).Id)
                return BadRequest();
            if (phoneNumber is null)
                return NotFound();
            _unitOfWork.Context.PhoneNumbers.Remove(phoneNumber);
            _unitOfWork.SaveChanges();
            return NoContent();
        }
    }
}