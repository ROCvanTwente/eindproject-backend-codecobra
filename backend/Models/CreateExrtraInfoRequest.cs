namespace backend.Models
{
    public class CreateExtraInformationRequest
    {
        public required string Title { get; set; }
        public required string Description { get; set; }

        public List<MediaRequest>? Media { get; set; }
    }

    public class MediaRequest
    {
        public int Id { get; set; }
    }
}