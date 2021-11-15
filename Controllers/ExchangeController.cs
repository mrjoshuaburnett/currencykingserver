using AutoMapper;
using CurrencyKing.Data.DatabaseModels;
using CurrencyKing.Data.ViewModels;
using CurrencyKing.Services;
using CurrencyKing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CurrencyKing.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ExchangeController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly DatabaseContext _context;
        private readonly ExchangeService _exchangeService;
        private readonly IMapper _mapper;
        public ExchangeController(IMapper mapper, DatabaseContext context, UserService userService, PasswordResetService passwordResetService, SecurityService securityService, EmailService emailService, ExchangeService exchangeService)
        {
            _userService = userService;
            _context = context;
            _exchangeService = exchangeService;
            _mapper = mapper;

        }


        [HttpGet("GetExchanges")]
        public async Task<ActionResult> GetExchanges()
        {
            var user = await _userService.GetCurrentUser(_context);

            var exchanges = await _exchangeService.GetByUser(_context, user.Id);

            return Ok(exchanges);
        }

        [HttpPost("CreateOrUpdate")]
        public async Task<ActionResult> CreateOrUpdate(UserExchangeModel userExchangeModel)
        {
            var user = await _userService.GetCurrentUser(_context);

            userExchangeModel.UserId = user.Id;

            await _exchangeService.CreateOrUpdate(_context, userExchangeModel);

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var user = await _userService.GetCurrentUser(_context);

            var exchanges = _exchangeService.Delete(_context, id);

            return Ok(exchanges);
        }
    }
}
