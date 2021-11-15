using CurrencyKing.Constants;
using CurrencyKing.Data.DatabaseModels;
using CurrencyKing.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyKing.Data
{
    public static class DbInitialiser
    {
        public static async Task Initialise(DatabaseContext context)
        {
            await context.Database.MigrateAsync();

            if (!await context.Users.AnyAsync())
            {
                var hashUtility = new HashUtility();
                hashUtility.GetHashAndSaltString("Password1234$", out string passwordHash, out string passwordSalt);

                var systemUser = new User
                {
                    EmailAddress = "mrjoshuaburnett@gmail.com",
                    Role = RoleConstant.Administrator,
                    FullName = "Joshua Burnett",
                    CreatedDate = DateTime.UtcNow,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt
                };


                await context.Users.AddAsync(systemUser);
            }

            await context.SaveChangesAsync();
        }
    }
}
