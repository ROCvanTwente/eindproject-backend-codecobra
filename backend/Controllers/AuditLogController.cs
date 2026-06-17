using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AuditLogController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> LogAction([FromBody] AuditLogRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body cannot be null.");
            }

            // Fallback chain: Token Name -> Request Body Actor -> Anonymous
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
    }
}