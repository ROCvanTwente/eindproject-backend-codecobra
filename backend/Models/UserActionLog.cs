using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class UserActionLog
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Actor { get; set; } = string.Empty;

        [Required]
        [MaxLength(80)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Target { get; set; } = string.Empty;

        public string? Metadata { get; set; }

        [MaxLength(300)]
        public string? Page { get; set; }

        [MaxLength(400)]
        public string? UserAgent { get; set; }

        [MaxLength(64)]
        public string? IpAddress { get; set; }

        public DateTime OccurredAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
