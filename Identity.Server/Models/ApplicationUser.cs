using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Identity.Server.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string GithubName { get; set; }
    }
}