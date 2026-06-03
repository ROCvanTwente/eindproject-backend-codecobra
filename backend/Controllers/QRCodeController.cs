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

        /// <summary>
        /// Haal de TourStop op die aan een QR code gekoppeld is
        /// GET /api/qrcode/{id}/tourstop
        /// </summary>
        [HttpGet("{id}/tourstop")]
        public async Task<IActionResult> GetTourStopByQRCode(int id)
        {
            var qrCode = await _context.QRCodes.FindAsync(id);
            if (qrCode == null)
                return NotFound(new { message = "QR Code not found" });

            var tourStop = await _context.TourStops
                .FirstOrDefaultAsync(t => t.QRCodeId == id);
            
            if (tourStop == null)
                return NotFound(new { message = "No tour stop found for this QR code" });

            return Ok(tourStop);
        }

        /// <summary>
        /// Maak een nieuwe TourStop aan
        /// POST /api/qrcode/tourstop/add
        /// </summary>
        [HttpPost("tourstop/add")]
        public async Task<IActionResult> CreateTourStop([FromBody] CreateTourStopRequest request)
        {
            // Validatie
            if (request.QRCodeId <= 0)
                return BadRequest(new { message = "QRCodeId is required" });

            // Check of QRCode bestaat
            var qrCode = await _context.QRCodes.FindAsync(request.QRCodeId);
            if (qrCode == null)
                return NotFound(new { message = "QR Code not found" });

            // Check of TourStop al bestaat voor deze QRCode
            var existingTourStop = await _context.TourStops
                .FirstOrDefaultAsync(t => t.QRCodeId == request.QRCodeId);
            if (existingTourStop != null)
                return BadRequest(new { message = "Tour stop already exists for this QR code" });

            // Maak nieuwe TourStop
            var tourStop = new TourStop
            {
                QRCodeId = request.QRCodeId,
                LocationNl = request.LocationNl,
                LocationEn = request.LocationEn,
                TitleNl = request.TitleNl,
                TitleEn = request.TitleEn,
                DescriptionNl = request.DescriptionNl,
                DescriptionEn = request.DescriptionEn,
                PositionX = request.PositionX,
                PositionY = request.PositionY,
                EstimatedDuration = request.EstimatedDuration,
                MediaUrl = request.MediaUrl,
                CreatedAt = System.DateTime.UtcNow,
                UpdatedAt = System.DateTime.UtcNow
            };

            _context.TourStops.Add(tourStop);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTourStopByQRCode), new { id = tourStop.QRCodeId }, tourStop);
        }
    }
}