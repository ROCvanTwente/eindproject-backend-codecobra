using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class QRCode
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Code is verplicht")]
    [StringLength(500)]
    public required string Code { get; set; }

    [Required(ErrorMessage = "Naam is verplicht")]
    [StringLength(200)]
    public required string Name { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Key relatie
    public ICollection<QRCodeStatistic> Statistics { get; set; } = new List<QRCodeStatistic>();
}
