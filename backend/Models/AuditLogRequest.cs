namespace backend.Models
{
    public class AuditLogRequest
    {
        public string? Actor { get; set; }
        public string? Action { get; set; }
        public string? Target { get; set; }
        public string? Metadata { get; set; }
        public string? Page { get; set; }
        public string? UserAgent { get; set; }
        public DateTime OccurredAtUtc { get; set; }
    }
}
