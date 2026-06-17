using System.ComponentModel.DataAnnotations;

namespace backend.Models;
public class Pronunciation
{
	public int Id { get; set; }

	[Required]
	public required string Word { get; set; }
	[Required]
	public required string PronunciationText { get; set; }
	[Required]
	public required string Language { get; set; } // e.g., "en", "nl", etc.
}
