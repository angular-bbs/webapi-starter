using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Identity.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Identity.Server.Controller
{
    [Route(ServerConsts.AccountApiBaseAddress)]
    [Authorize]
    public class AccountController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IHostingEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            HttpClient httpClient, IHostingEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _env = env;
        }
        
        [Route(ServerConsts.AccountApi.ChangePasswordEndpoint)]
        [HttpPost]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Request body is not valid");
            }
            if (model.NewPassword != model.ConfirmPassword)
            {
                return BadRequest("Confirm password does not match with password");
            }
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    return Ok();
                }
                return BadRequest(result.Errors);
            }
            return NotFound();
        }

        [Route(ServerConsts.AccountApi.ForgetPasswordEndpoint)]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Username is not valid");
            }

            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                if (_env.IsDevelopment())
                {
                    return NotFound();
                }
                return Ok();
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            // a url for client too call /api/account/reset-password with code.
            var callbackUrl = $"{ClientConsts.PasswordResetEndpoint}?code={code}&email={user.Email}";
            // Todo: replace with email.
            //await _emailSender.SendEmailAsync(model.Username, "Reset Password",
            //   $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
            if (_env.IsDevelopment())
            {
                return Ok(callbackUrl);
            }
            throw new NotImplementedException();
        }

        [Route(ServerConsts.AccountApi.ResetPasswordEndpoint)]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Form is not valid");
            }

            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest("Confirm Password does not match with password");
            }

            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                if (_env.IsDevelopment())
                {
                    return BadRequest("User not found");
                }
                return Ok();
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return Ok();
            }
            return BadRequest("Reset password failed.");
        }

        [Route(ServerConsts.AccountApi.RegisterEndpoint)]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(string username, string password)
        {
            // Get the information about the user from the external login provider
            // check if a local user is already registered, if not, register and signin.
            ApplicationUser user;
            try
            {
                user = await _userManager.FindByEmailAsync(username);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            if (user != null)
            {
                return BadRequest("User already registered.");
            }

            // if not registered
            user = new ApplicationUser {UserName = username, Email = username};

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                return Ok();
            }
            return Content("Failed to create user:" + user.Email);
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }
    }
}