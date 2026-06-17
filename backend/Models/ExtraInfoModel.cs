using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class ExtraInformation
    {
        public int Id { get; set; }

        [Required]
        public required string Title { get; set; }

        [Required]
        public required string Description { get; set; }

        public ICollection<Media> Media { get; set; }
            = new List<Media>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}