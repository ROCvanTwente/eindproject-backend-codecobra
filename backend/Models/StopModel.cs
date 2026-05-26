namespace backend.Models
{
	public class StopModel
	{
		public Guid Id { get; set; }
		public string QrCode { get; set; }
		public string Location_Nl { get; set; }
		public string Location_En { get; set; }
		public string Title_Nl { get; set; }
		public string Title_En { get; set; }
		public string Description_Nl { get; set; }
		public string Description_En { get; set; }
		public string? MediaUrl { get; set; } = null;
		public int Estimated_Duration { get; set; }
		public int Map_X { get; set; }
		public int Map_Y { get; set; }
		public int Order { get; set; } = 0;
	}
}
