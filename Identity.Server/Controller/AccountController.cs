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
        
    }
}