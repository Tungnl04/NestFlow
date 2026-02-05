using System.ComponentModel.DataAnnotations;

namespace NestFlow.Application.DTOs
{
    public class ResetPasswordDto
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string VerificationCode { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
    public class ForgotPasswordDto
    {
        public string Email { get; set; } = string.Empty;
    }
}
