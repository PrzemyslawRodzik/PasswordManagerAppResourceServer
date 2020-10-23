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
    public class AddressesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;


        public AddressesController(IMapper mapper, IUnitOfWork unitOfWork)
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
        
        // GET: api/address
        [HttpGet("addresss")]
        public ActionResult<IEnumerable<AddressDto>> GetAllAddresses()
        {
            List<Address> addresses = null;

            addresses = _unitOfWork.Context.Addresses.ToList();
           

            if (addresses.Count <= 0)
                return NoContent();

            var addressesDto = _mapper.Map<IEnumerable<AddressDto>>(addresses);
            return Ok(addressesDto);


        }

        // GET api/addresss/5
        [HttpGet("addresss/{id}", Name = "GetAddressById")]
        public ActionResult<AddressDto> GetAddressById(int id)
        {

            var address = _unitOfWork.Context.Addresses.Find(id);
            var addressDto = _mapper.Map<AddressDto>(address);
            if (address != null)
                return Ok(addressDto);

            return NotFound();


        }

        // POST api/addresss
        [HttpPost("addresss")]
        public ActionResult<AddressDto> CreateAddress([FromBody] AddressDto addressDto)
        {
            var address = _mapper.Map<Address>(addressDto);
            address.PersonalInfo = _unitOfWork.Context.PersonalInfos.FirstOrDefault(x => x.UserId == GetUserIdFromJwtToken());
            _unitOfWork.Context.Addresses.Add(address);
            _unitOfWork.SaveChanges();

            var addressDtoResult = _mapper.Map<AddressDto>(address);

            return CreatedAtRoute(nameof(GetAddressById), new { Id = address.Id }, addressDtoResult);

        }

        // PUT api/addresss/5
        [HttpPut("addresss/{id}")]
        public ActionResult UpdateAddress(int id, [FromBody] AddressDto addressDto)
        {
            var addressFromDb = _unitOfWork.Context.Addresses.Find(id);
            if (addressFromDb is null)
                return NotFound();

            _mapper.Map(addressDto, addressFromDb);
            
            _unitOfWork.Context.Addresses.Update(addressFromDb);
            _unitOfWork.SaveChanges();

            return NoContent();
        }

        // DELETE api/addresss/5
        [HttpDelete("addresss/{id}")]
        public ActionResult DeleteAddress(int id)
        {

            var address = _unitOfWork.Context.Addresses.Find(id);
            if (address.PersonalInfoId != _unitOfWork.Context.PersonalInfos.FirstOrDefault(x => x.UserId == GetUserIdFromJwtToken()).Id)
                return BadRequest();
            if (address is null)
                return NotFound();
            _unitOfWork.Context.Addresses.Remove(address);
            _unitOfWork.SaveChanges();
            return NoContent();
        }
    }
}