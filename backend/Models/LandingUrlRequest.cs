using System.ComponentModel.DataAnnotations;

public class LandingUrlRequest
{
    [Required(ErrorMessage = "URL is required")]
    [Url(ErrorMessage = "Invalid URL format")]
    [StringLength(2000)]
    public required string Url { get; set; }
}
