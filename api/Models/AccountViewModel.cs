using System.ComponentModel.DataAnnotations;

namespace WebapiStarter.Models
{
    public class AccountViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}