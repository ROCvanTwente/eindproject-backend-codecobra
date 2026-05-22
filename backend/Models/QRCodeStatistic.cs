using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class QRCodeStatistic
{
	public int Id { get; set; }
	[Required]
	[ForeignKey("QRCode")]
	public int QRCodeId { get; set; }
	public QRCode? QRCode { get; set; }

	[Required]
	public int ScanCount { get; set; } = 0;

	public DateTime LastScannedAt { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
