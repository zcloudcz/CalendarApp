using CalendarAppCore;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Text;

namespace CalendarApp.Api.Controllers
{
    /// <summary>
    /// API Controller for calendar generation and related operations.
    /// Provides endpoints for generating HTML calendars, uploading images, and fetching metadata.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CalendarController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CalendarController> _logger;

        /// <summary>
        /// Initializes the controller with environment and logging services.
        /// </summary>
        /// <param name="environment">Provides information about the web hosting environment</param>
        /// <param name="logger">Logger for recording errors and information</param>
        public CalendarController(IWebHostEnvironment environment, ILogger<CalendarController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Main endpoint for generating HTML calendars.
        /// Accepts form data including year, month, optional image/template uploads, and configuration.
        /// </summary>
        /// <param name="request">Calendar generation request with all parameters</param>
        /// <returns>Generated HTML calendar file as download</returns>
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateCalendar([FromForm] CalendarRequest request)
        {
            try
            {
                // Input validation - ensure year and month are in valid ranges
                if (request.Year < 2000 || request.Year > 2100)
                    return BadRequest(new { error = "Rok musí být mezi 2000 a 2100." });

                if (request.Month < 1 || request.Month > 12)
                    return BadRequest(new { error = "Měsíc musí být mezi 1 a 12." });

                var options = new MonthCalendarOptions
                {
                    Year = request.Year,
                    Month = request.Month,
                    RenderMode = request.RenderMode,
                    Region = request.Region
                };

                // Handle image upload
                if (request.ImageFile != null)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(request.ImageFile.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(extension))
                        return BadRequest(new { error = "Neplatný typ souboru. Povoleny jsou pouze JPG, PNG a GIF." });

                    // Validate file size (5MB max)
                    if (request.ImageFile.Length > 5 * 1024 * 1024)
                        return BadRequest(new { error = "Velikost souboru překračuje limit 5MB." });

                    var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadsPath);

                    var fileName = $"{Guid.NewGuid()}_{request.ImageFile.FileName}";
                    var filePath = Path.Combine(uploadsPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.ImageFile.CopyToAsync(stream);
                    }

                    // Use the server URL for the image path
                    options.ImagePath = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
                }
                else if (!string.IsNullOrEmpty(request.ImageUrl))
                {
                    options.ImagePath = request.ImageUrl;
                }

                // Handle template
                if (request.TemplateFile != null)
                {
                    // Validate template file size (1MB max)
                    if (request.TemplateFile.Length > 1024 * 1024)
                        return BadRequest(new { error = "Velikost template souboru překračuje limit 1MB." });

                    using var reader = new StreamReader(request.TemplateFile.OpenReadStream());
                    options.TemplateContent = await reader.ReadToEndAsync();
                }
                else if (!string.IsNullOrEmpty(request.TemplateContent))
                {
                    options.TemplateContent = request.TemplateContent;
                }

                var calendar = new MonthCalendar(options);
                var html = await calendar.BuildCalendarAsync();

                var outputFileName = $"kalendar-{request.Year}-{request.Month:D2}.html";
                var bytes = Encoding.UTF8.GetBytes(html);

                return File(bytes, "text/html", outputFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating calendar");
                return BadRequest(new { error = $"Chyba při generování kalendáře: {ex.Message}" });
            }
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = "No file uploaded" });

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { error = "Invalid file type. Only JPG, PNG, and GIF are allowed." });

                // Validate file size (5MB max)
                if (file.Length > 5 * 1024 * 1024)
                    return BadRequest(new { error = "File size exceeds 5MB limit." });

                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsPath);

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Ok(new { url = $"/uploads/{fileName}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("default-template")]
        public IActionResult GetDefaultTemplate()
        {
            // Return the default template content
            var options = new MonthCalendarOptions();
            var calendar = new MonthCalendar(options);
            return Ok(new { template = calendar.Template });
        }

        /// <summary>
        /// Retrieves a list of available regions for school holidays.
        /// Used by the frontend to populate the region dropdown.
        /// </summary>
        /// <returns>JSON array of region names</returns>
        [HttpGet("regions")]
        public async Task<IActionResult> GetRegions()
        {
            try
            {
                var apiClient = new KurzyApiClient();
                var regions = await apiClient.GetRegionsAsync();
                return Ok(new { regions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching regions");
                // Return fallback regions if API call fails
                return Ok(new { regions = new List<string> { "Blansko", "Brno-město", "Brno-venkov", "Praha" } });
            }
        }
    }

    /// <summary>
    /// Request model for calendar generation.
    /// Supports both file uploads (images, templates) and direct content/URLs.
    /// </summary>
    public class CalendarRequest
    {
        /// <summary>Year for the calendar (e.g., 2024)</summary>
        public int Year { get; set; }

        /// <summary>Month for the calendar (1-12)</summary>
        public int Month { get; set; }

        /// <summary>Optional uploaded image file (max 5MB, formats: jpg, png, gif)</summary>
        public IFormFile? ImageFile { get; set; }

        /// <summary>Optional image URL (alternative to ImageFile)</summary>
        public string? ImageUrl { get; set; }

        /// <summary>Optional uploaded template file (max 1MB)</summary>
        public IFormFile? TemplateFile { get; set; }

        /// <summary>Optional template content as string (alternative to TemplateFile)</summary>
        public string? TemplateContent { get; set; }

        /// <summary>Render mode: Default (0) or TwoRows/TwoColumns (1)</summary>
        public MonthCalendarRenderMode RenderMode { get; set; }

        /// <summary>Optional region filter for school holidays (e.g., "Blansko")</summary>
        public string? Region { get; set; }
    }
}
