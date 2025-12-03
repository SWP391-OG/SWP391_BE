using System.ComponentModel.DataAnnotations;

namespace SWP391.Contracts.Authentication
{
    public class VerifyEmailRequestDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Verification code is required")]
        public string VerificationCode { get; set; }
    }
}