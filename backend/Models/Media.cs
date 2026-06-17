using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class Media
{
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public required string FileName { get; set; }

    [Required]
    public required string FilePath { get; set; }

    [Required]
    [StringLength(50)]
    public required string FileType { get; set; } // "image/jpeg", "image/png", etc.

    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Relatie naar QRCode (optioneel)
    public int? QRCodeId { get; set; }
    public QRCode? QRCode { get; set; }

    // Relatie naar ExtraInformationMedia (optioneel)
    public int? ExtraInformationId { get; set; }
    public ExtraInformation? ExtraInformation { get; set; }
}
