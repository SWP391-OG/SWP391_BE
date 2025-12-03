using System.ComponentModel.DataAnnotations;

namespace SWP391.Contracts.Authentication
{
    public class ResendVerificationRequestDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
    }
}