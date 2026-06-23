using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class CreateUserDto
    {
        [Required]
        public string Username { get; set; } = default!;

        public string? Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = default!;

        public string Role { get; set; } = "Editor";
    }
}
