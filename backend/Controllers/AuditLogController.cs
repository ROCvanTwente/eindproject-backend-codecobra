using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/audit-logs")]
    public class AuditLogController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AuditLogController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserActionLog>>> GetHistory()
        {
            var logs = await _db.UserActionLogs
                .OrderByDescending(x => x.OccurredAtUtc)
                .ThenByDescending(x => x.CreatedAtUtc)
                .ToListAsync();

            return Ok(logs);
        }

        [HttpPost]
        public async Task<IActionResult> LogAction([FromBody] AuditLogRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body cannot be null.");
            }

            var actorFromToken = HttpContext.User?.Identity?.Name;
            var finalActor = string.IsNullOrWhiteSpace(actorFromToken)
                ? (string.IsNullOrWhiteSpace(request.Actor) ? "Anonymous" : request.Actor)
                : actorFromToken;

            _db.UserActionLogs.Add(new UserActionLog
            {
                Actor = finalActor,
                Action = request.Action ?? "Unknown",
                Target = request.Target ?? "Unknown",
                Metadata = request.Metadata,
                Page = request.Page,
                UserAgent = request.UserAgent,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                OccurredAtUtc = request.OccurredAtUtc == default ? DateTime.UtcNow : request.OccurredAtUtc,
                CreatedAtUtc = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> ClearHistory()
        {
            var logs = await _db.UserActionLogs.ToListAsync();
            _db.UserActionLogs.RemoveRange(logs);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}