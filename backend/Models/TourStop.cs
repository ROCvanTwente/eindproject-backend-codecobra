using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class TourStop
{
    public Guid Id { get; set; }

    [ForeignKey("QRCode")]
    public int QRCodeId { get; set; }
    public QRCode? QRCode { get; set; }

    [StringLength(500)]
    public string? LocationNl { get; set; }

    [StringLength(500)]
    public string? LocationEn { get; set; }

    [StringLength(200)]
    public string? TitleNl { get; set; }

    [StringLength(200)]
    public string? TitleEn { get; set; }

    [StringLength(2000)]
    public string? DescriptionNl { get; set; }

    [StringLength(2000)]
    public string? DescriptionEn { get; set; }

    public double? PositionX { get; set; }
    public double? PositionY { get; set; }

	public string? MediaUrl { get; set; }

	public int? EstimatedDuration { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int? Order { get; set; }
}
