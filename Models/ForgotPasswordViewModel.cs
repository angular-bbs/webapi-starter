using System.ComponentModel.DataAnnotations;

namespace WebapiStarter.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}