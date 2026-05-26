using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class CreateUserDto
    {
        [Required]
        public string Username { get; set; } = default!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = default!;

        public string Role { get; set; } = "Editor";
    }

    public class UserResponseDto
    {
        public string Id { get; set; } = default!;
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Role { get; set; } = default!;
    }
}
