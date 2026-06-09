using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using backend.Data;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QRCodeController : ControllerBase
    {
        private readonly IQRCodeStatisticService _statisticService;
        private readonly AppDbContext _context;

        // Guard tegen dubbele QR code aanvragen (in-memory cache voor snelle dubbele requests)
        private static readonly ConcurrentDictionary<string, byte> _pendingQRCodes = new();

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
                return BadRequest(new { message = "Code is required", statusCode = 400 });

            // Guard: Check of code al in aanmaak is (dubbele request bescherming)
            if (!_pendingQRCodes.TryAdd(request.Code, 0))
            {
                return Conflict(new { message = "QR Code creation already in progress. Please wait.", statusCode = 409 });
            }

            try
            {
                // Check of code al bestaat in database
                var existingQrCode = await _context.QRCodes
                    .FirstOrDefaultAsync(q => q.Code == request.Code);

                if (existingQrCode != null)
                {
                    return BadRequest(new { message = "QR Code already exists", statusCode = 400 });
                }

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
            finally
            {
                // Verwijder uit pending set
                _pendingQRCodes.TryRemove(request.Code, out _);
            }
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

        /// <summary>
        /// Haal de landing page URL op
        /// GET /api/qrcode/landing-url
        /// </summary>
        [HttpGet("landing-url")]
        public async Task<IActionResult> GetLandingUrl()
        {
            var setting = await _context.AppSettings
                .FirstOrDefaultAsync(s => s.Key == AppSetting.Keys.LandingPageUrl);

            if (setting == null)
            {
                return Ok(new LandingUrlResponse 
                { 
                    Url = null, 
                    UpdatedAt = DateTime.UtcNow 
                });
            }

            return Ok(new LandingUrlResponse 
            { 
                Url = setting.Value, 
                UpdatedAt = setting.UpdatedAt 
            });
        }

        /// <summary>
        /// Sla of update de landing page URL op
        /// PUT /api/qrcode/landing-url
        /// </summary>
        [HttpPut("landing-url")]
        public async Task<IActionResult> UpdateLandingUrl([FromBody] LandingUrlRequest request)
        {
            // Validatie
            if (string.IsNullOrWhiteSpace(request.Url))
            {
                return BadRequest(new { message = "URL cannot be empty", statusCode = 400 });
            }

            try
            {
                // Zoek bestaande setting
                var setting = await _context.AppSettings
                    .FirstOrDefaultAsync(s => s.Key == AppSetting.Keys.LandingPageUrl);

                if (setting == null)
                {
                    // Maak nieuwe setting aan
                    setting = new AppSetting
                    {
                        Key = AppSetting.Keys.LandingPageUrl,
                        Value = request.Url,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.AppSettings.Add(setting);
                }
                else
                {
                    // Update bestaande setting
                    setting.Value = request.Url;
                    setting.UpdatedAt = DateTime.UtcNow;
                    _context.AppSettings.Update(setting);
                }

                await _context.SaveChangesAsync();

                return Ok(new LandingUrlResponse 
                { 
                    Url = setting.Value, 
                    UpdatedAt = setting.UpdatedAt 
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { message = "Failed to update landing URL", details = ex.Message, statusCode = 500 });
            }
        }
    }
}