using System.ComponentModel.DataAnnotations;

namespace Identity.Server.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}