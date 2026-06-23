namespace backend.Models
{
	public class ReorderStopRequest
	{
		public int Order { get; set; }
		public DateTime UpdatedAt { get; set; } = DateTime.Now;
	}
}
