using AutoMapper;
using CurrencyKing.Constants;
using CurrencyKing.Data.DatabaseModels;
using CurrencyKing.Data.ViewModels;
using CurrencyKing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CurrencyKing.Services
{
    public class ExchangeService : BaseService<UserExchange, UserExchangeModel>
    {
        private readonly HashUtility _hashUtility;
        private readonly EmailService _emailUtility;
        private readonly PasswordResetService _passwordResetService;
        private readonly SecurityService _securityService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ExchangeService(IMapper mapper) : base(mapper)
        {
        }

        public async Task<List<UserExchangeModel>> GetByUser(DatabaseContext context, Guid id)
        {
            var test = _mapper;
            return await context.UserExchanges.Where(x => x.UserId == id).OrderByDescending(x=>x.CreatedDate).Select(a => _mapper.Map<UserExchangeModel>(a)).ToListAsync();
        }

    }
}
