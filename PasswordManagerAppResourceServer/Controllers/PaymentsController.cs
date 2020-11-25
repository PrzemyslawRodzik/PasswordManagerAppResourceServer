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
    public class PaymentsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;


        public PaymentsController(IMapper mapper, IUnitOfWork unitOfWork)
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
        
        
        // GET: api/paypalaccounts
        [HttpGet("paypalaccounts")]
        public ActionResult<IEnumerable<PaypalAccountDto>> GetAllPaypalAccounts([FromQuery] int? userId,
        [FromQuery] int? compromised)
        {
            List<PaypalAccount> paypalAccounts = null;
            if (userId is null && compromised is null)
                paypalAccounts = _unitOfWork.Context.PaypalAccounts.ToList();
            else if (userId != null && compromised is null)
                paypalAccounts = _unitOfWork.Context.PaypalAccounts.
                Where(x => x.UserId == userId).ToList();
            else if (userId != null && compromised == 1)
                paypalAccounts = _unitOfWork.Context.PaypalAccounts.
                Where(x => x.UserId == userId && x.Compromised == 1).ToList();
            else if (userId != null && compromised == 0)
                paypalAccounts = _unitOfWork.Context.PaypalAccounts.
                Where(x => x.UserId == userId && x.Compromised == 0).ToList();

            if (paypalAccounts.Count <= 0)
                return NoContent();

            var paypalAccountsDto = _mapper.Map<IEnumerable<PaypalAccountDto>>(paypalAccounts);
            return Ok(paypalAccountsDto);
        }

        // GET api/paypalaccounts/5
        [HttpGet("paypalaccounts/{id}", Name = "GetPaypalAccountById")]
        public ActionResult<PaypalAccountDto> GetPaypalAccountById(int id)
        {

            var paypalAccount = _unitOfWork.Context.PaypalAccounts.Find(id);
            var paypalAccountDto = _mapper.Map<PaypalAccountDto>(paypalAccount);
            if (paypalAccount != null)
                return Ok(paypalAccountDto);

            return NotFound();


        }

        // POST api/paypalaccounts
        [HttpPost("paypalaccounts")]
        public ActionResult<PaypalAccountDto> CreatePaypalAccount([FromBody] PaypalAccountDto paypalAccountDto)
        {
            var paypalAccount = _mapper.Map<PaypalAccount>(paypalAccountDto);
            paypalAccount.User = _unitOfWork.Users.Find<User>(GetUserIdFromJwtToken());
            paypalAccount.ModifiedDate = DateTime.UtcNow;
            _unitOfWork.Context.PaypalAccounts.Add(paypalAccount);
            _unitOfWork.SaveChanges();

            var paypalAccountDtoResult = _mapper.Map<PaypalAccountDto>(paypalAccount);

            return CreatedAtRoute(nameof(GetPaypalAccountById), new { Id = paypalAccount.Id }, paypalAccountDtoResult);

        }

        // PUT api/paypalaccounts/5
        [HttpPut("paypalaccounts/{id}")]
        public ActionResult UpdatePaypalAccount(int id, [FromBody] PaypalAccountDto paypalAccountDto)
        {
            var paypalAccountFromDb = _unitOfWork.Context.PaypalAccounts.Find(id);
            if (paypalAccountFromDb is null)
                return NotFound();

            _mapper.Map(paypalAccountDto, paypalAccountFromDb);
            paypalAccountFromDb.ModifiedDate = DateTime.UtcNow;
            _unitOfWork.Context.PaypalAccounts.Update(paypalAccountFromDb);
            _unitOfWork.SaveChanges();

            return NoContent();
        }

        // DELETE api/paypalaccounts/5
        [HttpDelete("paypalaccounts/{id}")]
        public ActionResult DeletePaypalAccount(int id)
        {

            var paypalAccount = _unitOfWork.Context.PaypalAccounts.Find(id);
            if (paypalAccount.UserId  != GetUserIdFromJwtToken())
                return BadRequest();
            if (paypalAccount is null)
                return NotFound();
            _unitOfWork.Context.PaypalAccounts.Remove(paypalAccount);
            _unitOfWork.SaveChanges();
            return NoContent();
        }



        // GET: api/creditcards
        [HttpGet("creditcards")]
        public ActionResult<IEnumerable<CreditCardDto>> GetAllCreditCards([FromQuery]int? userId)
        {
            List<CreditCard> creditCards = null;

            creditCards = _unitOfWork.Context.CreditCards.Where(x=>x.UserId==userId).ToList();
            if (creditCards.Count <= 0)
                return NoContent();

            var creditCardsDto = _mapper.Map<IEnumerable<CreditCardDto>>(creditCards);
            return Ok(creditCardsDto);
        }

        // GET api/creditcards/5
        [HttpGet("creditcards/{id}", Name = "GetCreditCardById")]
        public ActionResult<CreditCardDto> GetCreditCardById(int id)
        {

            var creditCard = _unitOfWork.Context.CreditCards.Find(id);
            var creditCardDto = _mapper.Map<CreditCardDto>(creditCard);
            if (creditCard != null)
                return Ok(creditCardDto);

            return NotFound();


        }

        // POST api/creditcards
        [HttpPost("creditcards")]
        public ActionResult<CreditCardDto> CreateCreditCard([FromBody] CreditCardDto creditCardDto)
        {
            var creditCard = _mapper.Map<CreditCard>(creditCardDto);
            creditCard.User = _unitOfWork.Users.Find<User>(GetUserIdFromJwtToken());
            _unitOfWork.Context.CreditCards.Add(creditCard);
            _unitOfWork.SaveChanges();

            var creditCardDtoResult = _mapper.Map<CreditCardDto>(creditCard);

            return CreatedAtRoute(nameof(GetCreditCardById), new { Id = creditCard.Id }, creditCardDtoResult);

        }

        // PUT api/creditcards/5
        [HttpPut("creditcards/{id}")]
        public ActionResult UpdateCreditCard(int id, [FromBody] CreditCardDto creditCardDto)
        {
            var creditCardFromDb = _unitOfWork.Context.CreditCards.Find(id);
            if (creditCardFromDb is null)
                return NotFound();

            _mapper.Map(creditCardDto, creditCardFromDb);

            _unitOfWork.Context.CreditCards.Update(creditCardFromDb);
            _unitOfWork.SaveChanges();

            return NoContent();
        }

        // DELETE api/creditcards/5
        [HttpDelete("creditcards/{id}")]
        public ActionResult DeleteCreditCard(int id)
        {

            var creditCard = _unitOfWork.Context.CreditCards.Find(id);
            if (creditCard.UserId  != GetUserIdFromJwtToken())
                return BadRequest();
            if (creditCard is null)
                return NotFound();
            _unitOfWork.Context.CreditCards.Remove(creditCard);
            _unitOfWork.SaveChanges();
            return NoContent();
        }

    

    }
}