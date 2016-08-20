using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Identity.Server.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;

namespace Identity.Server.Configurations
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResourceOwnerPasswordValidator(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<CustomGrantValidationResult> ValidateAsync(string userName, string password,
            ValidatedTokenRequest request)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null || !(await _userManager.CheckPasswordAsync(user, password)))
            {
                return new CustomGrantValidationResult("Username or password incorrect");
            }
            var claims = await _userManager.GetClaimsAsync(user);
            var sub = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            if (sub == null)
            {
                return new CustomGrantValidationResult("No subject is found");
            }
            return new CustomGrantValidationResult(sub, "password");
        }
    }
}