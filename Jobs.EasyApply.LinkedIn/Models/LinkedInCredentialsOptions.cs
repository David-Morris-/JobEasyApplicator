using System.ComponentModel.DataAnnotations;

namespace Jobs.EasyApply.LinkedIn.Models
{
    public class LinkedInCredentialsOptions
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(1, ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}
