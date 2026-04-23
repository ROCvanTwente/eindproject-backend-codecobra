namespace DigitaleRondleiding.Models
{
    public class Locatie
    {
        public int Id { get; set; }
        public string Naam { get; set; } = string.Empty;
        public string Beschrijving { get; set; } = string.Empty;
        public string? AfbeeldingUrl { get; set; }
        public int Volgorde { get; set; }
    }
}
