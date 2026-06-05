using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class UpdateUserRoleDto
    {
        [Required]
        public string NewRole { get; set; }
    }
}
