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

                // Parse excluded days from JSON if provided
                if (!string.IsNullOrEmpty(request.ExcludedDays))
                {
                    try
                    {
                        var excludedDtos = System.Text.Json.JsonSerializer.Deserialize<List<SpecialDayDto>>(request.ExcludedDays);
                        if (excludedDtos != null)
                        {
                            options.ExcludedDays = excludedDtos.Select(dto => new ExcludedDay
                            {
                                Date = DateTime.Parse(dto.Date),
                                Type = dto.Type,
                                Name = dto.Name
                            }).ToList();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse ExcludedDays JSON");
                    }
                }

                // Parse custom days from JSON if provided
                if (!string.IsNullOrEmpty(request.CustomDays))
                {
                    try
                    {
                        var customDtos = System.Text.Json.JsonSerializer.Deserialize<List<SpecialDayDto>>(request.CustomDays);
                        if (customDtos != null)
                        {
                            options.CustomDays = customDtos.Select(dto => new CustomDay
                            {
                                Date = DateTime.Parse(dto.Date),
                                Name = dto.Name,
                                Type = dto.Type ?? "custom"
                            }).ToList();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse CustomDays JSON");
                    }
                }

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
                else if (!string.IsNullOrEmpty(request.TemplateId))
                {
                    // Use template by ID from the template library
                    options.TemplateContent = CalendarTemplates.GetTemplateById(request.TemplateId);
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

        /// <summary>
        /// Gets a list of all available template variants.
        /// </summary>
        /// <returns>JSON array with template metadata (id, name, description, preview)</returns>
        [HttpGet("templates")]
        public IActionResult GetTemplates()
        {
            var templates = CalendarTemplates.GetAllTemplates();
            return Ok(new { templates });
        }

        /// <summary>
        /// Gets template content by ID.
        /// </summary>
        /// <param name="id">Template ID (default, minimal, colorful, classic, compact)</param>
        /// <returns>Template HTML content</returns>
        [HttpGet("templates/{id}")]
        public IActionResult GetTemplateById(string id)
        {
            var template = CalendarTemplates.GetTemplateById(id);
            var info = CalendarTemplates.GetTemplateInfo(id);

            return Ok(new {
                template = template,
                info = info
            });
        }

        /// <summary>
        /// Downloads template as HTML file for editing.
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>HTML file download</returns>
        [HttpGet("templates/{id}/download")]
        public IActionResult DownloadTemplate(string id)
        {
            var template = CalendarTemplates.GetTemplateById(id);
            var info = CalendarTemplates.GetTemplateInfo(id);
            var fileName = $"sablona-{info?.Name ?? id}.html";

            var bytes = Encoding.UTF8.GetBytes(template);
            return File(bytes, "text/html", fileName);
        }

        [HttpGet("default-template")]
        public IActionResult GetDefaultTemplate()
        {
            // Return the default template content
            var template = CalendarTemplates.GetTemplateById("default");
            return Ok(new { template });
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

        /// <summary>
        /// Retrieves special days (holidays, school holidays, special commemorative days, international days)
        /// for a specific year and month. Used for displaying and managing special days in the calendar.
        /// </summary>
        /// <param name="year">Year (2000-2100)</param>
        /// <param name="month">Month (1-12)</param>
        /// <param name="region">Optional region filter for school holidays</param>
        /// <returns>JSON object containing all special days for the specified month</returns>
        [HttpGet("special-days")]
        public async Task<IActionResult> GetSpecialDays([FromQuery] int year, [FromQuery] int month, [FromQuery] string? region = null)
        {
            try
            {
                // Input validation
                if (year < 2000 || year > 2100)
                    return BadRequest(new { error = "Rok musí být mezi 2000 a 2100." });

                if (month < 1 || month > 12)
                    return BadRequest(new { error = "Měsíc musí být mezi 1 a 12." });

                // Create API client for the specified year
                var apiClient = new KurzyApiClient(year);

                // Fetch all special days from the API
                var holidays = await apiClient.GetHolidaysAsync();
                var schoolHolidays = await apiClient.GetShoolHolidaysAsync();
                var specialDays = await apiClient.GetSpecialDaysAsync();
                var internationalDays = await apiClient.GetInternationalDaysAsync();

                // Calculate date range for the requested month
                var firstDay = new DateTime(year, month, 1);
                var lastDay = new DateTime(year, month, DateTime.DaysInMonth(year, month));

                // Filter days to only include those in the specified month
                var filteredHolidays = holidays
                    .Where(h => h.Start.HasValue && h.Start.Value >= firstDay && h.Start.Value <= lastDay && !string.IsNullOrEmpty(h.Name))
                    .Select(h => new SpecialDayDto
                    {
                        Date = h.Start!.Value.ToString("yyyy-MM-dd"),
                        Name = h.Name ?? "",
                        Type = "holiday"
                    })
                    .ToList();

                var filteredSchoolHolidays = schoolHolidays
                    .Where(h => h.Start.HasValue && h.Start.Value <= lastDay && h.End.HasValue && h.End.Value >= firstDay && !string.IsNullOrEmpty(h.Name))
                    .Where(h => string.IsNullOrEmpty(region) || string.IsNullOrEmpty(h.Region) || string.Equals(h.Region, region, StringComparison.InvariantCultureIgnoreCase))
                    .Select(h => new SpecialDayDto
                    {
                        Date = h.Start!.Value.ToString("yyyy-MM-dd"),
                        EndDate = h.End?.ToString("yyyy-MM-dd"),
                        Name = h.Name ?? "",
                        Type = "schoolHoliday",
                        Region = h.Region
                    })
                    .ToList();

                var filteredSpecialDays = specialDays
                    .Where(h => h.Start.HasValue && h.Start.Value >= firstDay && h.Start.Value <= lastDay && !string.IsNullOrEmpty(h.Name))
                    .Select(h => new SpecialDayDto
                    {
                        Date = h.Start!.Value.ToString("yyyy-MM-dd"),
                        Name = h.Name ?? "",
                        Type = "specialDay"
                    })
                    .ToList();

                var filteredInternationalDays = internationalDays
                    .Where(h => h.Start.HasValue && h.Start.Value >= firstDay && h.Start.Value <= lastDay && !string.IsNullOrEmpty(h.Name))
                    .Select(h => new SpecialDayDto
                    {
                        Date = h.Start!.Value.ToString("yyyy-MM-dd"),
                        Name = h.Name ?? "",
                        Type = "internationalDay"
                    })
                    .ToList();

                return Ok(new
                {
                    holidays = filteredHolidays,
                    schoolHolidays = filteredSchoolHolidays,
                    specialDays = filteredSpecialDays,
                    internationalDays = filteredInternationalDays
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching special days for {Year}-{Month}", year, month);
                return BadRequest(new { error = $"Chyba při načítání speciálních dnů: {ex.Message}" });
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

        /// <summary>Template ID from the template library (default, minimal, colorful, classic, compact)</summary>
        public string? TemplateId { get; set; }

        /// <summary>Render mode: Default (0) or TwoRows/TwoColumns (1)</summary>
        public MonthCalendarRenderMode RenderMode { get; set; }

        /// <summary>Optional region filter for school holidays (e.g., "Blansko")</summary>
        public string? Region { get; set; }

        /// <summary>
        /// JSON string containing excluded special days (dates to be removed from the calendar).
        /// Format: array of SpecialDayDto objects serialized to JSON
        /// </summary>
        public string? ExcludedDays { get; set; }

        /// <summary>
        /// JSON string containing custom special days added by the user.
        /// Format: array of SpecialDayDto objects serialized to JSON
        /// </summary>
        public string? CustomDays { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for special days (holidays, school holidays, special days, international days).
    /// Used for API communication between frontend and backend.
    /// </summary>
    public class SpecialDayDto
    {
        /// <summary>Date in format yyyy-MM-dd (e.g., "2025-01-01")</summary>
        public required string Date { get; set; }

        /// <summary>Optional end date for multi-day events (e.g., school holidays)</summary>
        public string? EndDate { get; set; }

        /// <summary>Name/description of the special day</summary>
        public required string Name { get; set; }

        /// <summary>
        /// Type of special day: "holiday", "schoolHoliday", "specialDay", "internationalDay", "custom"
        /// </summary>
        public required string Type { get; set; }

        /// <summary>Optional region (for school holidays)</summary>
        public string? Region { get; set; }
    }
}
