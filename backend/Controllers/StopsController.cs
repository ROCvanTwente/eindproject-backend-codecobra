using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class StopsController : ControllerBase
	{
		private readonly AppDbContext _context;

		public StopsController(AppDbContext context)
		{
			_context = context;
		}

		/// <summary>
		/// Haal alle tour stops op
		/// GET /api/stops/all
		/// </summary>
		[HttpGet("all")]
		public async Task<IActionResult> GetAllStops()
		{
			var stops = await _context.TourStops
				.Include(t => t.QRCode)
				.ToListAsync();

			return Ok(stops);
		}

		/// <summary>
		/// Haal een specifieke tour stop op
		/// GET /api/stops/{id}
		/// </summary>
		[HttpGet("{id}")]
		public async Task<IActionResult> GetStopById(int id)
		{
			var stop = await _context.TourStops.FindAsync(id);
			if (stop == null)
				return NotFound(new { message = "Tour stop not found" });

			return Ok(stop);
		}

		/// <summary>
		/// Maak een nieuwe tour stop aan
		/// POST /api/stops/add
		/// </summary>
		[HttpPost("add")]
		public async Task<IActionResult> CreateStop([FromForm] CreateTourStopRequest request)
		{
			// We negeren de automatische binding van request.QRCodeId, 
			// omdat de frontend nu de tekst via de key "qrCode" meestuurt.
			if (ModelState.ContainsKey(nameof(request.QRCodeId)))
			{
				ModelState.Remove(nameof(request.QRCodeId));
			}

			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			// 1. Bereken het automatische order nummer
			var maxOrder = await _context.TourStops
				.MaxAsync(t => (int?)t.Order) ?? 0;
			var newOrder = maxOrder + 1;

			// 2. Lees de RUWE TEXT (bijv. "gieterij-001") uit de nieuwe key "qrCode"
			string qrCodeText = Request.Form["qrCode"].ToString()?.Trim();

			if (string.IsNullOrWhiteSpace(qrCodeText))
			{
				return BadRequest(new { message = "Het 'qrCode' veld (de tekst) is verplicht." });
			}

			// 3. Zoek of maak de QRCode op basis van de tekst ("gieterij-001")
			// Dit levert het automatisch gegenereerde database-ID op (bijv. 1)
			int generatedDatabaseId = await EnsureQrCodeExistsAsync(qrCodeText, request.TitleNl ?? $"QR voor {request.LocationNl}");

			// 4. Uniekheidscheck: Is dit database-ID (bijv. 1) al gekoppeld aan een andere stop?
			var stopUsingQr = await _context.TourStops
				.FirstOrDefaultAsync(t => t.QRCodeId == generatedDatabaseId);

			if (stopUsingQr != null)
			{
				return BadRequest(new { message = $"De QR-code '{qrCodeText}' is al gekoppeld aan een andere stop." });
			}

			// 5. Maak de nieuwe tour stop aan
			var tourStop = new TourStop
			{
				QRCodeId = generatedDatabaseId, // Hier vullen we de int (1) in!
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
				Order = newOrder,
				CreatedAt = System.DateTime.UtcNow,
				UpdatedAt = System.DateTime.UtcNow
			};

			_context.TourStops.Add(tourStop);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetStopById), new { id = tourStop.Id }, tourStop);
		}

		/// <summary>
		/// Update een bestaande tour stop
		/// PUT /api/stops/{id}
		/// </summary>
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateStop(int id, [FromForm] UpdateTourStopRequest request)
		{
			var stop = await _context.TourStops.FindAsync(id);
			if (stop == null)
				return NotFound(new { message = "Tour stop not found" });

			if (ModelState.ContainsKey(nameof(request.QRCodeId)))
			{
				ModelState.Remove(nameof(request.QRCodeId));
			}

			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			// Optioneel: QR code updaten (alleen als opgegeven en anders dan huidige)
			string qrCodeString = Request.Form["qrCode"].ToString()?.Trim();
			if (string.IsNullOrWhiteSpace(qrCodeString))
			{
				qrCodeString = request.QRCodeId?.Trim();
			}

			if (!string.IsNullOrWhiteSpace(qrCodeString))
			{
				// Zoek QR code op
				var qrCode = await _context.QRCodes.FirstOrDefaultAsync(q => q.Code == qrCodeString);

				if (qrCode == null)
				{
					// Maak QR code aan als die niet bestaat
					qrCode = new QRCode
					{
						Code = qrCodeString,
						Name = request.TitleNl ?? qrCodeString,
						CreatedAt = System.DateTime.UtcNow
					};
					_context.QRCodes.Add(qrCode);
					await _context.SaveChangesAsync();
				}

				// Safety check: alleen koppelen als die QR code nog vrij is OF al aan dit stop gekoppeld is
				if (stop.QRCodeId != qrCode.Id)
				{
					var otherStopWithQrCode = await _context.TourStops
						.Where(t => t.QRCodeId == qrCode.Id && t.Id != id)
						.FirstOrDefaultAsync();

					if (otherStopWithQrCode != null)
					{
						// QR code is al in gebruik - hou huidige koppeling
						return BadRequest(new { message = $"QR code '{qrCodeString}' is al in gebruik door ander stop. Koppeling niet gewijzigd." });
					}

					// OK - koppel de QR code
					stop.QRCodeId = qrCode.Id;
				}
			}

			// Update alle andere velden veilig
			if (!string.IsNullOrEmpty(request.LocationNl)) stop.LocationNl = request.LocationNl;
			if (!string.IsNullOrEmpty(request.LocationEn)) stop.LocationEn = request.LocationEn;
			if (!string.IsNullOrEmpty(request.TitleNl)) stop.TitleNl = request.TitleNl;
			if (!string.IsNullOrEmpty(request.TitleEn)) stop.TitleEn = request.TitleEn;
			if (!string.IsNullOrEmpty(request.DescriptionNl)) stop.DescriptionNl = request.DescriptionNl;
			if (!string.IsNullOrEmpty(request.DescriptionEn)) stop.DescriptionEn = request.DescriptionEn;

			if (request.PositionX.HasValue && request.PositionX >= 0) stop.PositionX = request.PositionX;
			if (request.PositionY.HasValue && request.PositionY >= 0) stop.PositionY = request.PositionY;
			if (request.EstimatedDuration.HasValue && request.EstimatedDuration > 0) stop.EstimatedDuration = request.EstimatedDuration;
			if (request.MediaUrl != null)
			{
				stop.MediaUrl = string.IsNullOrWhiteSpace(request.MediaUrl) ? null : request.MediaUrl.Trim();
			}

			stop.UpdatedAt = System.DateTime.UtcNow;

			_context.TourStops.Update(stop);
			await _context.SaveChangesAsync();

			return Ok(stop);
		}

		/// <summary>
		/// Update alleen de media URL van een stop
		/// PUT /api/stops/{id}/media
		/// </summary>
		[HttpPut("{id}/media")]
		public async Task<IActionResult> UpdateStopMedia(int id, [FromBody] UpdateStopMediaRequest request)
		{
			var stop = await _context.TourStops.FindAsync(id);
			if (stop == null)
				return NotFound(new { message = "Tour stop not found" });

			stop.MediaUrl = string.IsNullOrWhiteSpace(request.MediaUrl)
				? null
				: request.MediaUrl.Trim();
			stop.UpdatedAt = System.DateTime.UtcNow;

			_context.TourStops.Update(stop);
			await _context.SaveChangesAsync();

			return Ok(stop);
		}

		/// <summary>
		/// Verwijder een tour stop
		/// DELETE /api/stops/{id}
		/// </summary>
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteStop(int id)
		{
			var stop = await _context.TourStops.FindAsync(id);
			if (stop == null)
				return NotFound(new { message = "Tour stop not found" });

			_context.TourStops.Remove(stop);
			await _context.SaveChangesAsync();

			return Ok(new { message = "Tour stop deleted successfully" });
		}

		private async Task<int> EnsureQrCodeExistsAsync(string codeString, string defaultName)
		{
			// We zoeken in de database op de STRING kolom genaamd 'Code'
			var existingQr = await _context.QRCodes
				.FirstOrDefaultAsync(q => q.Code == codeString);

			if (existingQr != null)
			{
				return existingQr.Id; // Geeft de bestaande INT id terug (bijv. 1)
			}

			// Bestaat hij nog niet? Dan maken we hem exact zo aan als in jouw voorbeeld:
			var newQrCode = new QRCode
			{
				Code = codeString,  // "gieterij-001"
				Name = codeString,  // In jouw voorbeeld is Name gelijk aan Code: "gieterij-001"
				CreatedAt = System.DateTime.UtcNow
			};

			_context.QRCodes.Add(newQrCode);
			await _context.SaveChangesAsync(); // SQL Server genereert nu automatisch Id = 1

			return newQrCode.Id; // Geeft de kersverse INT id (1) terug
		}

		public class UpdateStopMediaRequest
		{
			public string? MediaUrl { get; set; }
		}
	}
}
