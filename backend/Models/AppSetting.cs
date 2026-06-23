using System;
using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class AppSetting
{
    public int Id { get; set; }

    [Required]
    public required string Key { get; set; }

    [Required]
    public required string Value { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}