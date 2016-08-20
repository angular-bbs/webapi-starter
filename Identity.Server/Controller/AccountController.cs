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
using WebapiStarter.GitHub;
using WebapiStarter.Models;

namespace WebapiStarter.Controllers
{
    [Route(ServerConsts.AccountApiBaseAddress)]
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IHostingEnvironment _env;
        private readonly HttpClient _httpClient;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private string _accessToken;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            HttpClient httpClient, IHostingEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpClient = httpClient;
            _accessToken = GithubConfig.GetToken(httpContextAccessor);
            if (!string.IsNullOrEmpty(_accessToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            }
            _env = env;
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Angular-bbs");
        }

        [Route(ServerConsts.AccountApi.CheckStatusEndpoint)]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CheckStatusAsync()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }
            var hasPassword = await _userManager.HasPasswordAsync(user);
            return Ok(new UserModel
            {
                HasPassword = hasPassword,
                Name = user.GithubName,
                Email = user.Email
            });
        }

        [Route("token")]
        [HttpGet]
        public string GetToken()
        {
            if (_env.IsDevelopment())
            {
                if (string.IsNullOrEmpty(_accessToken))
                {
                    return "Access token is null.";
                }
                return _accessToken;
            }
            return "Not available in production";
        }

        [Route(ServerConsts.AccountApi.LoginGithubEndpoint)]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginGithubAsync([FromBody] GithubRequestViewModel model)
        {
            if (string.IsNullOrEmpty(model.Code))
            {
                return BadRequest("Code is not supplied");
            }

            var response = await _httpClient.PostAsJsonAsync(GithubEndpoints.TokenExchangeEndpoint, new
            {
                client_id = GithubConfig.ClientId,
                client_secret = GithubConfig.Secret,
                code = model.Code,
                redirect_uri = model.RedirectUrl,
                state = model.State
            });
            response.EnsureSuccessStatusCode();
            var content = JObject.Parse(await response.Content.ReadAsStringAsync());
            var token = content.Value<string>("access_token");
            if (token == null)
            {
                return BadRequest("Code is not Valid");
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            response = await _httpClient.GetAsync(GithubEndpoints.UserEndpoint);
            response.EnsureSuccessStatusCode();
            content = JObject.Parse(await response.Content.ReadAsStringAsync());
            return await LoginOrCreateLocalAccountAsync(content, token);
        }

        [Route(ServerConsts.AccountApi.LoginLocallyEndpoint)]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> LoginLocallyAsync([FromBody] LoginViewModel model)
        {
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest("Username or password not provided");
            }
            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, true, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                return Ok(new UserModel
                {
                    Name = user?.GithubName,
                    Email = model.Username,
                    HasPassword = true
                });
            }
            if (result.IsLockedOut)
            {
                return BadRequest("User account locked out");
            }
            return BadRequest("Username or password is not valid");
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
                    await _signInManager.SignInAsync(user, isPersistent: true);
                    return Ok();
                }
                return BadRequest(result.Errors);
            }
            return NotFound();
        }

        [Route(ServerConsts.AccountApi.CreatePasswordEndpoint)]
        [HttpPost]
        public async Task<IActionResult> CreatePasswordAsync([FromBody] CreatePasswordViewModel model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest("Passord does not match confirm password.");
            }
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return BadRequest("User not found");
            }
            var result = await _userManager.AddPasswordAsync(user, model.Password);
            if (result.Succeeded)
            {
                return Ok();
            }
            return BadRequest("Failed to create password");
        }

        [Route(ServerConsts.AccountApi.LogoutEndpoint)]
        [HttpPost]
        public async Task<IActionResult> LogOffAsync()
        {
            await _signInManager.SignOutAsync();
            return Ok();
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

        private async Task<IActionResult> LoginOrCreateLocalAccountAsync(JToken content, string token)
        {
            // Get the information about the user from the external login provider
            var email = content.Value<string>("email");
            var name = content.Value<string>("name");
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name))
            {
                return BadRequest("Username or name is not available");
            }
            // check if a local user is already registered, if not, register and signin.
            ApplicationUser user;
            try
            {
                user = await _userManager.FindByEmailAsync(email);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            if (user != null)
            {
                var claims = await _userManager.GetClaimsAsync(user);
                foreach (var claim in claims)
                {
                    if (claim.Type == GithubConfig.TokenClaimType)
                    {
                        await _userManager.RemoveClaimAsync(user, claim);
                    }
                }
                await _userManager.AddClaimAsync(user, new Claim(GithubConfig.TokenClaimType, token));
                await _signInManager.SignInAsync(user, isPersistent: true);
                var hasPassword = await _userManager.HasPasswordAsync(user);
                return Ok(new UserModel
                {
                    HasPassword = hasPassword,
                    Name = user.GithubName,
                    Email = user.UserName
                });
            }

            // if not registered, register a new user with user's github email address.
            user = new ApplicationUser {UserName = email, Email = email, GithubName = name};

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                await _userManager.AddClaimAsync(user, new Claim(GithubConfig.TokenClaimType, token));
                await _signInManager.SignInAsync(user, true);
                return Ok(new UserModel
                {
                    HasPassword = false,
                    Name = user.GithubName,
                    Email = user.UserName
                });
            }
            return Content("Failed to create user:" + user.Email);
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }
    }
}