using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExtraInformationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExtraInformationController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/extrainformation/add
        [HttpPost("add")]
        public async Task<IActionResult> Create([FromBody] CreateExtraInformationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest(new { message = "Title is required" });

            if (string.IsNullOrWhiteSpace(request.Description))
                return BadRequest(new { message = "Description is required" });

            var info = new ExtraInformation
            {
                Title = request.Title,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.ExtraInformations.Add(info);
            await _context.SaveChangesAsync();

            if (request.Media != null)
            {
                foreach (var m in request.Media)
                {
                    var media = await _context.Medias.FindAsync(m.Id);

                    if (media != null)
                    {
                        media.ExtraInformationId = info.Id;
                    }
                }

                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetById),
                new { id = info.Id }, info);
        }

        // GET: api/extrainformation/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var infos = await _context.ExtraInformations
                .Include(e => e.Media)
                .ToListAsync();
            return Ok(infos);
        }

        // GET: api/extrainformation/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var info = await _context.ExtraInformations
                .Include(e => e.Media)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (info == null)
                return NotFound(new { message = "Extra information not found" });

            return Ok(info);
        }

        // PUT: api/extrainformation/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateExtraInformationRequest request)
        {
            var info = await _context.ExtraInformations
                .Include(e => e.Media)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (info == null)
                return NotFound(new { message = "Extra information not found" });

            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest(new { message = "Title is required" });

            if (string.IsNullOrWhiteSpace(request.Description))
                return BadRequest(new { message = "Description is required" });

            info.Title = request.Title;
            info.Description = request.Description;

            await _context.SaveChangesAsync();

            return Ok(info);
        }

        // DELETE: api/extrainformation/delete/{id}
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var info = await _context.ExtraInformations.FindAsync(id);

            if (info == null)
                return NotFound(new { message = "Extra information not found" });

            _context.ExtraInformations.Remove(info);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Extra information deleted successfully" });
        }
    }
}