using System.ComponentModel.DataAnnotations;

public class AppSetting
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Key is verplicht")]
    [StringLength(100)]
    public required string Key { get; set; }

    [StringLength(2000)]
    public string? Value { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Constraint: Key moet uniek zijn
    public static class Keys
    {
        public const string LandingPageUrl = "LANDING_PAGE_URL";
    }
}
