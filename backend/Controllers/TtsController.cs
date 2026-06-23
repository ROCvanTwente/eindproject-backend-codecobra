using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class TtsController : Controller
	{
		private readonly AppDbContext _context;
		
		public TtsController(AppDbContext context)
		{
			_context = context;
		}

		[HttpGet("all")]
		public async Task<IActionResult> GetAllPronunciations()
		{
			var pronunciations = await _context.Pronunciations.ToListAsync();
			return Ok(pronunciations);
		}

		[HttpPost("add")]
		public async Task<IActionResult> AddPronunciation([FromBody] Pronunciation pronunciation)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			_context.Pronunciations.Add(pronunciation);
			await _context.SaveChangesAsync();
			return CreatedAtAction(nameof(GetAllPronunciations), new { id = pronunciation.Id }, pronunciation);
		}

		[HttpPut]
		public async Task<IActionResult> UpdatePronunciation([FromBody] Pronunciation pronunciation)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var existingPronunciation = await _context.Pronunciations.FindAsync(pronunciation.Id);
			if (existingPronunciation == null)
			{
				return NotFound(new { message = "Pronunciation not found" });
			}
			existingPronunciation.Word = pronunciation.Word;
			existingPronunciation.PronunciationText = pronunciation.PronunciationText;
			existingPronunciation.Language = pronunciation.Language;
			await _context.SaveChangesAsync();
			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeletePronunciation(int id)
		{
			var pronunciation = await _context.Pronunciations.FindAsync(id);
			if (pronunciation == null)
			{
				return NotFound(new { message = "Pronunciation not found" });
			}
			_context.Pronunciations.Remove(pronunciation);
			await _context.SaveChangesAsync();
			return NoContent();
		}
	}
}
