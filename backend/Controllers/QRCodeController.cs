using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using backend.Services;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QRCodeController : ControllerBase
    {
        private readonly IQRCodeStatisticService _statisticService;

        public QRCodeController(IQRCodeStatisticService statisticService)
        {
            _statisticService = statisticService;
        }

        [HttpPost("scan")]
        public async Task<IActionResult> ScanQRCode([FromBody] ScanRequest request)
        {
            await _statisticService.RecordScanAsync(request.QrCode);
            return Ok(new { message = "Scan recorded successfully" });
        }

        [HttpGet("statistics/{qrCodeId}")]
        public async Task<IActionResult> GetStatistics(int qrCodeId)
        {
            var stats = await _statisticService.GetStatisticsAsync(qrCodeId);
            if (stats == null)
                return NotFound(new { message = "No statistics found" });

            return Ok(stats);
        }
    }

    public class ScanRequest
    {
        public required string QrCode { get; set; }
    }
}
