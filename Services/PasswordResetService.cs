using AutoMapper;
using CurrencyKing.Data.DatabaseModels;
using CurrencyKing.Data.ViewModels;
using CurrencyKing.Utilities;
using Hangfire;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CurrencyKing.Services
{
    public class PasswordResetService : BaseService<PasswordReset, PasswordResetModel>
    {

        public PasswordResetService(IMapper mapper) : base(mapper)
        {
        }
        public async Task<PasswordReset> Create(DatabaseContext context, Guid userId)
        {
            var reset = new PasswordReset()
            {
                UserId = userId,
                Expiry = DateTime.UtcNow.AddMinutes(10),
            };

            await context.PasswordResets.AddAsync(reset);
            await context.SaveChangesAsync();
            return reset;
        }


        public async Task Remove(DatabaseContext context, Guid id)
        {
            var reset = await GetConcrete(context, id);

            context.PasswordResets.Remove(reset);
            await context.SaveChangesAsync();
        }

    }
}
