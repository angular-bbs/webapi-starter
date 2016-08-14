using System.ComponentModel.DataAnnotations;

namespace WebapiStarter.Models
{
    public class GithubRequestViewModel
    {
        public string State { get; set; }
        [Required]
        public string Code { get; set; }
        public string RedirectUrl { get; set; }
    }
}