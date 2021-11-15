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
    public class UserService : BaseService<User, UserModel>
    {
        private readonly HashUtility _hashUtility;
        private readonly EmailService _emailUtility;
        private readonly PasswordResetService _passwordResetService;
        private readonly SecurityService _securityService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IConfiguration _config, PasswordResetService passwordResetService, SecurityService securityService, IHttpContextAccessor httpContextAccessor, IMapper mapper) : base(mapper)
        {
            _hashUtility = new HashUtility();
            _emailUtility = new EmailService(_config);
            _passwordResetService = passwordResetService;
            _securityService = securityService;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<User> Create(DatabaseContext context, UserModel user)
        {
            Expression<Func<UserModel, bool>> conflictCheck = u => u.EmailAddress == user.EmailAddress;

            var userEntity = await CreateOrUpdate(context, user);
            var reset = await _passwordResetService.Create(context, userEntity.Id);

            await _emailUtility.SendSignUpEmail(userEntity, reset);

            return userEntity;
        }

        public async Task<User> GetByEmailAddress(DatabaseContext context, string emailAddress)
        {
            return await context.Users.Where(u => u.EmailAddress == emailAddress).SingleOrDefaultAsync();
        }
        public async Task<User> CreateOnSignUp(DatabaseContext context, string email, string name)
        {
            Expression<Func<User, bool>> conflictCheck = u => u.EmailAddress == email;
            var user = new UserModel()
            {
                EmailAddress = email,
                FullName = name,
                Role = RoleConstant.Standard
            };
            return await CreateOrUpdate(context, user);
        }
        public bool VerifyPassword(string password, string salt, string hash)
        {
            return _hashUtility.VerifyHashString(password, hash, salt);
        }

        public async Task UpdateRefreshToken(DatabaseContext context, User userIn)
        {
            var entity = await Get(context, userIn.Id).SingleOrDefaultAsync();

            entity.RefreshToken = userIn.RefreshToken;
        }

        public async Task<User> GetCurrentUser(DatabaseContext databaseContext)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var accessToken = httpContext.Request.Cookies["accessToken"];
            var accessTokenClaims = _securityService.GetJwtClaims(accessToken);
            var userId = accessTokenClaims?.FindFirst("userId")?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }
            else
            {
                return await GetConcrete(databaseContext, Guid.Parse(userId));
            }
        }

        public async Task ResetPassword(DatabaseContext context, Guid Id, string newPassword)
        {
            var user = Get(context, Id).FirstOrDefault();

            _hashUtility.GetHashAndSaltString(newPassword, out string passwordHash, out string salt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = salt;

            await context.SaveChangesAsync();
        }

        public async Task<string> GetUserRefreshToken(DatabaseContext context, Guid Id)
        {
            try
            {
                var user = await Get(context, Id).SingleOrDefaultAsync();
                if (user != null)
                {
                    return user.RefreshToken;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task RevokeUserRefreshToken(DatabaseContext context, Guid userId)
        {
            var user = await Get(context, userId).SingleOrDefaultAsync();
            if (user != null)
            {
                user.RefreshToken = null;

                await context.SaveChangesAsync();
            }
        }
    }
}
