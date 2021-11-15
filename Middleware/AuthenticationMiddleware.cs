using CurrencyKing.Data.DatabaseModels;
using CurrencyKing.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyKing.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly UserService _userService;
        private readonly SecurityService _securityService;
        private readonly DatabaseContext _dbContext;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, UserService _userService, SecurityService _securityService, DatabaseContext _dbContext)
        {
            try
            {
                var path = httpContext.Request.Path.ToString();

                if (path != "/")
                {
                    var accessToken = httpContext.Request.Cookies["accessToken"];
                    var refreshToken = httpContext.Request.Cookies["refreshToken"];
                    if (!string.IsNullOrEmpty(refreshToken))
                    {
                        var accessTokenExpiry = !string.IsNullOrEmpty(accessToken) ? _securityService.GetTokenCustomExpiry(accessToken) : DateTime.Now.AddDays(-1).Ticks;

                        //if the access token has expired, check the refresh token is still valid and then issue a new access token
                        if (accessTokenExpiry < DateTime.Now.ToUniversalTime().Ticks)
                        {
                            //if the refresh token HAS NOT expired
                            var refreshTokenExpiry = _securityService.GetTokenCustomExpiry(refreshToken);
                            if (refreshTokenExpiry > DateTime.UtcNow.AddMinutes(5).Ticks)
                            {
                                var refreshTokenClaims = _securityService.GetJwtClaims(refreshToken);
                                var refreshTokenSecret = refreshTokenClaims?.FindFirst("secret")?.Value;
                                var userId = refreshTokenClaims?.FindFirst("userId")?.Value;
                                //this is a critical security point - we need to verify that the given refresh token matches the refresh token stored in the db.  if not, revoke access.
                                var storedRefreshToken = await _userService.GetUserRefreshToken(_dbContext, Guid.Parse(userId));
                                var isRefreshTokenVerified = storedRefreshToken != null && _securityService.VerifyRefreshTokensMatch(storedRefreshToken, refreshToken);

                                if (isRefreshTokenVerified)
                                {
                                    var user = await _userService.GetConcrete(_dbContext, Guid.Parse(userId));

                                    var newAccessToken = _securityService.GenerateAccessToken(user.Role.ToString(), userId, refreshTokenSecret);
                                    httpContext.Response.Cookies.Append("accessToken", newAccessToken, new CookieOptions { MaxAge = TimeSpan.FromMinutes(365), HttpOnly = true });
                                    httpContext.Request.Headers.Add("Authorization", "Bearer " + newAccessToken);
                                }
                                else
                                {
                                    //something fishy going on so revoke all tokens and delete cookies
                                    httpContext.Response.Cookies.Delete("accessToken");
                                    httpContext.Response.Cookies.Delete("refreshToken");
                                    await _userService.RevokeUserRefreshToken(_dbContext, Guid.Parse(userId));
                                }
                            }
                            else
                            {
                                var refreshTokenClaims = _securityService.GetJwtClaims(refreshToken);
                                var userId = refreshTokenClaims?.FindFirst("userId")?.Value;
                                var storedRefreshToken = await _userService.GetUserRefreshToken(_dbContext, Guid.Parse(userId));
                                var isRefreshTokenVerified = _securityService.VerifyRefreshTokensMatch(storedRefreshToken, refreshToken);

                                //if the db refresh token has not been invalidated and the client refresh token matches, we can re-issue a new refresh token.
                                if (isRefreshTokenVerified)
                                {
                                    var user = await _userService.GetConcrete(_dbContext, Guid.Parse(userId));
                                    var newAccessToken = _securityService.SetCookiesOnLogin(httpContext, user);
                                    await _userService.UpdateRefreshToken(_dbContext, user);

                                    httpContext.Request.Headers.Add("Authorization", "Bearer " + newAccessToken);
                                }
                                else
                                {
                                    httpContext.Response.Cookies.Delete("accessToken");
                                    httpContext.Response.Cookies.Delete("refreshToken");
                                    await _userService.RevokeUserRefreshToken(_dbContext, Guid.Parse(userId));
                                }
                            }
                        }
                        else
                        {
                            var refreshTokenClaims = _securityService.GetJwtClaims(accessToken);
                            var userId = refreshTokenClaims?.FindFirst("userId")?.Value;

                            if (httpContext.Request.Headers.ContainsKey("UserId"))
                                httpContext.Request.Headers["UserId"] = userId;
                            else
                                httpContext.Request.Headers.Add("UserId", userId);

                            if (httpContext.Request.Headers.ContainsKey("Authorization"))
                                httpContext.Request.Headers["Authorization"] = "Bearer " + accessToken;
                            else
                                httpContext.Request.Headers.Add("Authorization", "Bearer " + accessToken);
                        }
                    }
                }
            }
            catch (Exception)
            {
                var accessToken = httpContext.Request.Cookies["accessToken"];
                var refreshToken = httpContext.Request.Cookies["refreshToken"];
                var claims = !string.IsNullOrWhiteSpace(accessToken) ? _securityService.GetJwtClaims(accessToken) : _securityService.GetJwtClaims(refreshToken);

                if (claims != null)
                {
                    var userId = claims?.FindFirst("userId")?.Value;
                    await _userService.RevokeUserRefreshToken(_dbContext, Guid.Parse(userId));
                }

                httpContext.Response.Cookies.Delete("accessToken");
                httpContext.Response.Cookies.Delete("refreshToken");
            }

            await _next.Invoke(httpContext);
        }
    }

}
