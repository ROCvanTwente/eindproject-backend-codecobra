using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = default!;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(128, MinimumLength = 1, ErrorMessage = "Password is required")]
        public string Password { get; set; } = default!;
    }
}
