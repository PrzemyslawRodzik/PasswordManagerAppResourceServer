using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManagerAppResourceServer.Interfaces;
using PasswordManagerAppResourceServer.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PasswordManagerAppResourceServer.Controllers
{
    [Route("api/")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StatisticsController(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
       
        
        [AllowAnonymous]
        // POST api/visitoragents
        [HttpPost("visitoragents")]
        public ActionResult<VisitorAgent> CreateVisitorAgent([FromBody] VisitorAgent visitorAgent)
        {
            try
            {
                _unitOfWork.Context.VisitorAgents.Add(visitorAgent);
                _unitOfWork.SaveChanges();
                
            }
            catch (Exception)
            {
                return BadRequest(new { Success = false });
            }
            return Ok(new { Success = true });


        }


    }
}
