using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using backend.Data;
using backend.Services;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QRCodeController : ControllerBase
    {
        private readonly IQRCodeStatisticService _statisticService;
        private readonly AppDbContext _context;

        public QRCodeController(IQRCodeStatisticService statisticService, AppDbContext context)
        {
            _statisticService = statisticService;
            _context = context;
        }

        /// <summary>
        /// Maak een nieuwe QR code aan
        /// POST /api/qrcode/add
        /// </summary>
        [HttpPost("add")]
        public async Task<IActionResult> CreateQRCode([FromBody] CreateQRCodeRequest request)
        {
            // Validatie
            if (string.IsNullOrWhiteSpace(request.Code))
                return BadRequest(new { message = "Code is required" });

            // Check of code al bestaat
            var existingQrCode = await _context.QRCodes
                .FirstOrDefaultAsync(q => q.Code == request.Code);
            if (existingQrCode != null)
                return BadRequest(new { message = "QR Code already exists" });

            // Maak nieuwe QR code
            var qrCode = new QRCode
            {
                Code = request.Code,
                Name = request.Name ?? "Unnamed QR Code",
                CreatedAt = System.DateTime.UtcNow
            };

            _context.QRCodes.Add(qrCode);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQRCodeById), new { id = qrCode.Id }, qrCode);
        }

        /// <summary>
        /// Haal alle QR codes op
        /// GET /api/qrcode/all
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllQRCodes()
        {
            var qrCodes = await _context.QRCodes.ToListAsync();
            return Ok(qrCodes);
        }

        /// <summary>
        /// Haal een specifieke QR code op
        /// GET /api/qrcode/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQRCodeById(int id)
        {
            var qrCode = await _context.QRCodes.FindAsync(id);
            if (qrCode == null)
                return NotFound(new { message = "QR Code not found" });

            return Ok(qrCode);
        }

        /// <summary>
        /// Verwijder een QR code
        /// DELETE /api/qrcode/delete/{id}
        /// </summary>
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteQRCode(int id)
        {
            var qrCode = await _context.QRCodes.FindAsync(id);
            if (qrCode == null)
                return NotFound(new { message = "QR Code not found" });

            _context.QRCodes.Remove(qrCode);
            await _context.SaveChangesAsync();

            return Ok(new { message = "QR Code deleted successfully" });
        }

        /// <summary>
        /// Registreer een scan
        /// POST /api/qrcode/scan
        /// </summary>
        [HttpPost("scan")]
        public async Task<IActionResult> ScanQRCode([FromBody] ScanRequest request)
        {
            await _statisticService.RecordScanAsync(request.QrCode);
            return Ok(new { message = "Scan recorded successfully" });
        }

        /// <summary>
        /// Haal statistieken van een QR code op
        /// GET /api/qrcode/statistics/{qrCodeId}
        /// </summary>
        [HttpGet("statistics/{qrCodeId}")]
        public async Task<IActionResult> GetStatistics(int qrCodeId)
        {
            var stats = await _statisticService.GetStatisticsAsync(qrCodeId);
            if (stats == null)
                return NotFound(new { message = "No statistics found" });

            return Ok(stats);
        }
    }

    // Request Models
    public class ScanRequest
    {
        public required string QrCode { get; set; }
    }

    public class CreateQRCodeRequest
    {
        public required string Code { get; set; }
        public string? Name { get; set; }
    }
}
