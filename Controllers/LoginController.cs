using CurrencyKing.Data.DatabaseModels;
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
    public class LoginController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly PasswordResetService _passwordResetService;
        private readonly DatabaseContext _context;
        private readonly SecurityService _securityService;
        private readonly EmailService _emailService;
        public LoginController(DatabaseContext context, UserService userService, PasswordResetService passwordResetService, SecurityService securityService, EmailService emailService)
        {
            _userService = userService;
            _passwordResetService = passwordResetService;
            _context = context;
            _securityService = securityService;
            _emailService = emailService;

        }

        [AllowAnonymous]
        [HttpPost("AttemptLogin")]
        public async Task<ActionResult> AttemptLogin([FromForm] string email, [FromForm] string password)
        {
            try
            {
                var user = await _userService.GetByEmailAddress(_context, email);

                if (user != null && _userService.VerifyPassword(password, user.PasswordSalt, user.PasswordHash))
                {
                    _securityService.SetCookiesOnLogin(HttpContext, user);

                    await _userService.UpdateRefreshToken(_context, user);
                    await _context.SaveChangesAsync();

                    return new JsonResult(new { user.Id, user.Role, user.FullName, user.EmailAddress });
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                throw e;
            }


        }

        [HttpGet("Authenticate")]
        public async Task<ActionResult> Authenticate()
        {
            var user = await _userService.GetCurrentUser(_context);

            if (user == null)
                return Unauthorized();
            else
                return new JsonResult(new { user.Id, user.Role, user.FullName, user.EmailAddress });
        }

        [AllowAnonymous]
        [HttpPost("Signup")]
        public async Task<ActionResult> Signup([FromForm] string email, [FromForm] string name)
        {
            var user = await _userService.GetByEmailAddress(_context, email);

            if (user != null)
            {
                throw new Exception("A user with that email address already exists");
            }
            else
            {
                user = await _userService.CreateOnSignUp(_context, email, name);
            }
            await _context.SaveChangesAsync();

            _securityService.SetCookiesOnLogin(HttpContext, user);

            await _userService.UpdateRefreshToken(_context, user);
            await _context.SaveChangesAsync();

            return new JsonResult(new { user.Id, user.Role, user.FullName, user.EmailAddress });

        }

        [HttpGet("Logout")]
        public async Task<ActionResult> Logout()
        {
            var token = HttpContext.Request.Cookies["accessToken"];

            var claims = _securityService.GetJwtClaims(token);
            var userId = Guid.Parse(claims?.FindFirst("UserId")?.Value);
            var currentUser = await _userService.GetConcrete(_context, userId);

            _securityService.ClearCookiesOnLogout(HttpContext, currentUser);
            await _context.SaveChangesAsync();
            return Ok();
        }


        [AllowAnonymous]
        [HttpGet("ResetPassword")]
        public async Task<ActionResult> ResetPassword(string email)
        {
            try
            {
                var user = await _userService.GetByEmailAddress(_context, email);

                if (user == null)
                    return NotFound();

                var reset = await _passwordResetService.Create(_context, user.Id);
                var response = await _emailService.SendPasswordResetEmail(user, reset);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        [HttpGet("ChangePassword")]
        public async Task<ActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            var currentUser = await _userService.GetCurrentUser(_context);

            if (_userService.VerifyPassword(currentPassword, currentUser.PasswordSalt, currentUser.PasswordHash))
            {
                await _userService.ResetPassword(_context, currentUser.Id, newPassword);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, message = "Password Successfully Updated!" });
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return new JsonResult(new { error = "Unable to change password - The current password you provided is invalid.  Please logout and reset your password instead." });
            }
        }
    }
}
