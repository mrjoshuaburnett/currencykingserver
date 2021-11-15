using CurrencyKing.Data.DatabaseModels;
using CurrencyKing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyKing.Services
{
    public class SecurityService
    {
        private IConfiguration _config;
        private static Random random = new Random();

        public SecurityService(IConfiguration config)
        {
            _config = config;
        }

        public string SetCookiesOnLogin(HttpContext httpContext, User user)
        {
            var tokenSecret = RandomString(32);
            var refreshToken = GenerateRefreshToken(tokenSecret, user.Id.ToString());
            var accessToken = GenerateAccessToken(user.Role.ToString(), user.Id.ToString(), tokenSecret);
            user.RefreshToken = refreshToken;
            httpContext.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions { MaxAge = TimeSpan.FromDays(365), HttpOnly = true });
            httpContext.Response.Cookies.Append("accessToken", accessToken, new CookieOptions { MaxAge = TimeSpan.FromMinutes(365), HttpOnly = true });
            return accessToken;
        }
        public void ClearCookiesOnLogout(HttpContext httpContext, User user)
        {
            user.RefreshToken = null;
            httpContext.Response.Cookies.Delete("refreshToken");
            httpContext.Response.Cookies.Delete("accessToken");
        }
        public string GenerateRefreshToken(string tokenSecret, string userId)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var exp = DateTime.Now.AddDays(365);
            var claims = new[]
            {
                new Claim("secret", tokenSecret),
                new Claim("userId", userId),
                new Claim("customExpiry", exp.Ticks.ToString()),
            };
            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              claims,
                expires: exp,
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public string GenerateAccessToken(string role, string userId, string tokenSecret)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var exp = DateTime.Now.AddMinutes(5);
            var claims = new[]
            {
                new Claim("role", role),
                new Claim("userId", userId),
                new Claim("secret", tokenSecret),
                new Claim("customExpiry", exp.Ticks.ToString()),
            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddDays(365),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal GetJwtClaims(string jwtToken)
        {
            try
            {
                SecurityToken validatedToken;
                TokenValidationParameters validationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    ValidAudience = _config["Jwt:Issuer"].ToLower(),
                    ValidIssuer = _config["Jwt:Issuer"].ToLower(),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
                };

                ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(jwtToken, validationParameters, out validatedToken);

                return principal;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public DateTime GetTokenExpiry(string tokenString)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadToken(tokenString) as JwtSecurityToken;
                var tokenExpiryDate = token.ValidTo;

                return tokenExpiryDate;
            }
            catch (Exception)
            {
                return new DateTime(1970, 1, 1);
            }
        }

        public long GetTokenCustomExpiry(string tokenString)
        {
            var dbClaims = GetJwtClaims(tokenString);
            var expiryClaim = dbClaims?.FindFirst("customExpiry")?.Value;
            var value = (long)Convert.ToDouble(expiryClaim);

            return value;
        }
        public bool VerifyRefreshTokensMatch(string dbRefreshToken, string clientRefreshToken)
        {
            var dbClaims = GetJwtClaims(dbRefreshToken);
            var clientClaims = GetJwtClaims(clientRefreshToken);
            var dbSecret = dbClaims?.FindFirst("secret")?.Value;
            var clientSecret = clientClaims?.FindFirst("secret")?.Value;
            return dbSecret == clientSecret;
        }

    }

}
