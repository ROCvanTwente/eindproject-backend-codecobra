public class UpdateTourStopRequest
{
    public string? LocationNl { get; set; }
    public string? LocationEn { get; set; }
    public string? TitleNl { get; set; }
    public string? TitleEn { get; set; }
    public string? DescriptionNl { get; set; }
    public string? DescriptionEn { get; set; }
    public string? MediaUrl { get; set; } = string.Empty;
	public double? PositionX { get; set; }
    public double? PositionY { get; set; }
    public int? EstimatedDuration { get; set; }
    public string? QRCodeId { get; set; }
}
