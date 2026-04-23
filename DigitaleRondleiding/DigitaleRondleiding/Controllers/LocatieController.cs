using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitaleRondleiding.Data;
using DigitaleRondleiding.Models;

namespace DigitaleRondleiding.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocatieController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LocatieController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Locatie>>> GetLocaties()
        {
            var locaties = await _context.Locaties
                .OrderBy(l => l.Volgorde)
                .ToListAsync();

            return Ok(locaties);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Locatie>> GetLocatie(int id)
        {
            var locatie = await _context.Locaties.FindAsync(id);

            if (locatie == null)
                return NotFound(new { message = $"Locatie met ID {id} niet gevonden." });

            return Ok(locatie);
        }
    }
}
