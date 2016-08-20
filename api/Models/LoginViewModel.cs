using System.ComponentModel.DataAnnotations;

namespace WebapiStarter.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}