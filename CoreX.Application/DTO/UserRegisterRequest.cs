using System.ComponentModel.DataAnnotations;

namespace CoreX.Application.DTO
{
    public class UserRegisterRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public required string FullName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public required string Password { get; set; }

        [Required]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public required string ConfirmPassword { get; set; }

        [Required]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms of use.")]
        public required bool TermsAccepted { get; set; }
    }
}
