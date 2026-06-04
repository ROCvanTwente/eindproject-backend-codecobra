using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Data;
using backend.Models;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MediaController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const long MaxFileSize = 50 * 1024 * 1024; // 50 MB
        private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".mp4", ".webm", ".mov", ".m4v" };

        public MediaController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadMedia(IFormFile file, int? qrCodeId = null)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Geen bestand geselecteerd");

            if (file.Length > MaxFileSize)
                return BadRequest($"Bestand mag maximaal {MaxFileSize / (1024 * 1024)} MB zijn");

            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(fileExtension))
                return BadRequest("Dit bestandstype is niet toegestaan. Gebruik: JPG, PNG, GIF, WebP, MP4, WebM, MOV, M4V");

            // Valideer QRCode als opgegeven
            if (qrCodeId.HasValue)
            {
                var qrCode = await _context.QRCodes.FindAsync(qrCodeId);
                if (qrCode == null)
                    return BadRequest("QR Code niet gevonden");
            }

            try
            {
                // Maak uploads directory
                var uploadsDir = Path.Combine(_webHostEnvironment.ContentRootPath, "uploads");
                Directory.CreateDirectory(uploadsDir);

                // Genereer unieke bestandsnaam
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsDir, uniqueFileName);

                // Sla bestand op
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Voeg Media toe aan database
                var media = new Media
                {
                    FileName = file.FileName,
                    FilePath = $"/uploads/{uniqueFileName}",
                    FileType = file.ContentType ?? "application/octet-stream",
                    FileSize = file.Length,
                    QRCodeId = qrCodeId
                };

                _context.Medias.Add(media);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    id = media.Id, 
                    fileName = media.FileName,
                    filePath = media.FilePath,
                    fileType = media.FileType,
                    fileSize = media.FileSize,
                    uploadedAt = media.UploadedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Fout bij uploaden: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMedia(int id)
        {
            var media = await _context.Medias.FindAsync(id);
            if (media == null)
                return NotFound();

            return Ok(new { 
                id = media.Id, 
                fileName = media.FileName,
                filePath = media.FilePath,
                fileType = media.FileType,
                fileSize = media.FileSize,
                uploadedAt = media.UploadedAt,
                qrCodeId = media.QRCodeId
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedia(int id)
        {
            var media = await _context.Medias.FindAsync(id);
            if (media == null)
                return NotFound();

            try
            {
                // Verwijder bestand
                var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, media.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                // Verwijder uit database
                _context.Medias.Remove(media);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Media verwijderd" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Fout bij verwijderen: " + ex.Message });
            }
        }
    }
}
