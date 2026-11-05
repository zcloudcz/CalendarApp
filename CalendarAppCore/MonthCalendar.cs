using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace CalendarAppCore
{
    /// <summary>
    /// Main class for generating HTML calendars with Czech holiday data.
    /// Supports multiple rendering modes and customizable templates.
    /// </summary>
    public class MonthCalendar
    {
        // API client for fetching holiday and nameday data
        KurzyApiClient _api = new();

        // Culture info for formatting dates and month names in Czech
        private readonly CultureInfo _culture;

        // Configuration options (year, month, template, render mode, etc.)
        private readonly MonthCalendarOptions _opts;

        // Collection of all days in the month with their metadata
        internal List<MonthCalendarDay> _days;

        // HTML template with placeholders for dynamic content
        internal string _template;

        /// <summary>
        /// Gets the HTML template being used for calendar generation.
        /// Useful for API endpoints that need to return the template.
        /// </summary>
        public string Template => _template;

        /// <summary>
        /// Gets the default file path for exporting the generated calendar.
        /// Format: "MMMM-yyyy.html" (e.g., "prosinec-2024.html")
        /// </summary>
        public string ExportFilePath => Path.Combine(Directory.GetCurrentDirectory(), new DateTime(_opts.Year, _opts.Month, 1).ToString("MMMM-yyyy") + ".html");

        /// <summary>
        /// Initializes a new calendar instance for a specific month and year.
        /// </summary>
        /// <param name="opts">Configuration options. If null, uses defaults (current month/year).</param>
        public MonthCalendar(MonthCalendarOptions opts = null)
        {
            _culture = CultureInfo.CurrentCulture;
            opts ??= new MonthCalendarOptions();
            this._opts = opts;
            _days = new();

            // Template loading priority:
            // 1. Direct template content (TemplateContent) - for web uploads
            // 2. Template ID from library (TemplateId) - for selecting built-in templates
            // 3. Template file path (TemplateFilePath) - for file system templates
            // 4. Built-in default template (GetDefaultTemplate) - fallback
            if (!string.IsNullOrEmpty(opts.TemplateContent))
            {
                _template = opts.TemplateContent;
            }
            else if (!string.IsNullOrEmpty(opts.TemplateId))
            {
                _template = CalendarTemplates.GetTemplateById(opts.TemplateId);
            }
            else if (!string.IsNullOrEmpty(opts.TemplateFilePath) && File.Exists(opts.TemplateFilePath))
            {
                _template = File.ReadAllText(opts.TemplateFilePath);
            }
            else
            {
                _template = CalendarTemplates.GetTemplateById("default");
            }

            // Create a MonthCalendarDay object for each day in the month
            for (var d = 1; d <= DateTime.DaysInMonth(opts.Year, opts.Month); d++)
                _days.Add(new(new(opts.Year, opts.Month, d)));
        }

        public override string ToString()
        {
            return BuildCalendarAsync().GetAwaiter().GetResult();
        }

        public async Task ToFileAsync(string filename = null)
        {
            filename ??= ExportFilePath; // Path.Combine(Directory.GetCurrentDirectory(), new DateTime(_opts.Year, _opts.Month, 1).ToString("MMMM-yyyy") + ".html");
            
            await File.WriteAllTextAsync(filename, await BuildCalendarAsync());
        }


        /// <summary>
        /// Generates the complete HTML calendar by fetching data from multiple sources
        /// and replacing placeholders in the template.
        /// This is the main method called by the API to generate calendars.
        /// </summary>
        /// <returns>Complete HTML string ready to be displayed or saved to a file.</returns>
        public async Task<string> BuildCalendarAsync()
        {
            // Fetch all required data in parallel from external API
            var names = await _api.GetNamesAsync();              // Czech namedays (e.g., "Jan", "Marie")
            var schoolHolidays = await _api.GetShoolHolidaysAsync(); // School holidays (region-specific)
            var holidays = await _api.GetHolidaysAsync();         // Public holidays (nationwide)
            var specialDays = await _api.GetSpecialDaysAsync();   // Special commemorative days
            var interDays = await _api.GetInternationalDaysAsync(); // International days (e.g., Earth Day)

            // Apply exclusions - remove special days that user wants to exclude
            holidays = ApplyExclusions(holidays, "holiday");
            schoolHolidays = ApplyExclusions(schoolHolidays, "schoolHoliday");
            specialDays = ApplyExclusions(specialDays, "specialDay");
            interDays = ApplyExclusions(interDays, "internationalDay");

            // Add custom special days from user input
            var customHolidays = ConvertCustomDaysToHolidays();

            var daySection = GetForeach();
            var daysInMonth = DateTime.DaysInMonth(_opts.Year, _opts.Month);
            var firstDay = new DateTime(_opts.Year, _opts.Month, 1);
            
            var days = new StringBuilder();

            foreach (var d in GetOrderDays())
            {
                var altCss = GetAltCss(d);

                string holidaysText = GetExtraText(d, holidays);
                string sHolidaysText = GetExtraText(d, schoolHolidays, true);
                string specialDaysText = GetExtraText(d, specialDays);
                string interDaysText = GetExtraText(d, interDays);

                // Add custom special days for this date
                string customDaysText = GetExtraText(d, customHolidays);

                var nms = names.Where(q => q.Date == d.Date).OrderByDescending(o => o.Count).DistinctBy(x => x.Name.RemoveDiacritics()).Take(2);
                if (nms.Count() > 1 && nms.Any(q => q.Count < 50)) nms = nms.Where(q => q.Count > 50);

                days.Append(
                    daySection.Replace("@alt", altCss)
                                .Replace("@date-name", d.Date.ToString("dddd"))
                                .Replace("@date-note", string.Join(" ", nms.Select(s => s.Name)))
                                .Replace("@date-holidays", holidaysText)
                                .Replace("@date-sholidays", sHolidaysText)
                                .Replace("@date-specialday", specialDaysText)
                                .Replace("@date-interday", interDaysText)
                                .Replace("@date-customday", customDaysText)  // Add custom days placeholder
                                .Replace("@date", $"{d.Date.Day}")
                                );
            }

            return _template.Replace("@image-url", _opts.ImagePath)
                            .Replace("@month-name", firstDay.ToString("MMMM"))
                            .Replace("@year", firstDay.ToString("yyyy"))
                            .Replace(GetForeach(true), days.ToString());

        }

        private string GetExtraText(MonthCalendarDay d, IEnumerable<Holiday> holidays, bool checkEndDate = false)
        {

            var hdays = holidays;

            if (checkEndDate)
                hdays = hdays.Where(q => q.Start <= d.Date && q.End >= d.Date);
                //sb.Append(string.Join(", ", holidays.Where(q => q.Start <= d.Date && q.End >= d.Date).Select(s => s.Name)));
            else
                hdays = hdays.Where(q => q.Start == d.Date);
                //sb.Append(string.Join(", ", holidays.Where(q => q.Start == d.Date).Select(s => s.Name)));

            if (!string.IsNullOrEmpty(_opts.Region))
            {
                hdays = hdays.Where(q => string.IsNullOrEmpty(q.Region) || string.Equals(q.Region, _opts.Region, StringComparison.InvariantCultureIgnoreCase));
            }


            StringBuilder sb = new();
            sb.Append(string.Join(", ", hdays.Select(s => s.Name)));

            return sb.ToString().Trim(',').Trim();
        }

        private IEnumerable<MonthCalendarDay> GetOrderDays()
        {
            if (_opts.RenderMode == MonthCalendarRenderMode.TwoRows)
            {
                for(var i = 1; i <= 15; i++)
                {
                    var alt = i % 2 == 0;

                    var day =  _days.First(q => q.Date.Day == i);
                    day.IsAlt = alt;
                    yield return day;

                    var day2 = _days.FirstOrDefault(q => q.Date.Day == i + 15);

                    if (day2 != null)
                    {
                        day2.IsAlt = alt;
                        yield return day2;
                    }
                }
                var last = _days.FirstOrDefault(q => q.Date.Day == 31);
                if (last != null) yield return last;
                yield break;
            }

            foreach(var r in _days.OrderBy(o => o.Date.Day))
                yield return r;
        }

        private string GetAltCss(MonthCalendarDay date)
        {
            StringBuilder sb = new();

            if (date.IsAlt) sb.Append(" day-alt");
            if (date.IsWeekend) sb.Append(" weekend");

            return sb.ToString();
        }

        private string GetForeach(bool full = false)
        {
            string start = "@foreach-begin";
            string end = "@foreach-end";

            var spi = _template.IndexOf(start, 0);

            if (spi == -1) return string.Empty;

            var epi = _template.IndexOf(end, spi);

            if (epi == -1) return string.Empty;

            var epos = epi-spi;

            return full
                ? _template.Substring(spi, epos + end.Length)
                : _template.Substring(spi + start.Length, epos - start.Length);

        }

        /// <summary>
        /// Applies user-defined exclusions to a list of holidays.
        /// Filters out any holidays that match the excluded dates and types.
        /// </summary>
        /// <param name="holidays">Original list of holidays from API</param>
        /// <param name="type">Type of holiday to check against exclusions</param>
        /// <returns>Filtered list with exclusions applied</returns>
        private List<Holiday> ApplyExclusions(List<Holiday> holidays, string type)
        {
            // If no exclusions defined, return original list
            if (_opts.ExcludedDays == null || !_opts.ExcludedDays.Any())
                return holidays;

            // Filter out holidays that match excluded entries
            return holidays.Where(h =>
            {
                // Check if this holiday should be excluded
                var shouldExclude = _opts.ExcludedDays.Any(ex =>
                    ex.Type == type &&
                    h.Start.HasValue &&
                    ex.Date.Date == h.Start.Value.Date &&
                    ex.Name == h.Name
                );

                return !shouldExclude; // Keep only non-excluded holidays
            }).ToList();
        }

        /// <summary>
        /// Converts user-defined custom days into Holiday objects for calendar rendering.
        /// This allows custom days to be processed the same way as API-fetched holidays.
        /// </summary>
        /// <returns>List of Holiday objects created from custom days</returns>
        private List<Holiday> ConvertCustomDaysToHolidays()
        {
            // If no custom days defined, return empty list
            if (_opts.CustomDays == null || !_opts.CustomDays.Any())
                return new List<Holiday>();

            // Convert CustomDay objects to Holiday objects
            return _opts.CustomDays.Select(cd => new Holiday
            {
                Start = cd.Date,
                End = cd.Date,
                Name = cd.Name,
                Region = null
            }).ToList();
        }

        [Obsolete("Use CalendarTemplates.GetTemplateById() instead")]
        private static string GetDefaultTemplate()
        {
            return @"<!DOCTYPE html>
<html lang=""cs"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Kalendář @month-name @year</title>
    <style>
        /* =================================================================
           GLOBÁLNÍ STYLY
           ================================================================= */
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 20px;
            min-height: 100vh;
        }

        /* =================================================================
           KONTEJNER KALENDÁŘE
           ================================================================= */
        .calendar-container {
            max-width: 1400px;
            margin: 0 auto;
            background: white;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
        }

        /* =================================================================
           HLAVIČKA S OBRÁZKEM
           ================================================================= */
        .calendar-header {
            position: relative;
            width: 100%;
            height: 400px;
            overflow: hidden;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        }

        .image-section {
            position: relative;
            width: 100%;
            height: 100%;
        }

        /* Oprava pro obrázek - zajistí správné zobrazení i při tisku */
        .month-image {
            width: 100%;
            height: 100%;
            object-fit: contain;
            object-position: center;
            display: block;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        }

        /* Název měsíce překrývá obrázek */
        .month-title {
            position: absolute;
            top: 50%;
            right: 60px;
            transform: translateY(-50%);
            font-size: 72px;
            font-weight: 900;
            color: white;
            text-shadow: 3px 3px 8px rgba(0, 0, 0, 0.7),
                         -1px -1px 2px rgba(0, 0, 0, 0.5);
            text-transform: uppercase;
            letter-spacing: 3px;
            line-height: 1.1;
            text-align: right;
        }

        /* =================================================================
           TĚLO KALENDÁŘE
           ================================================================= */
        .calendar-body {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            background: white;
            gap: 0;
        }

        /* =================================================================
           ŘÁDEK DNE
           ================================================================= */
        .calendar-row {
            display: grid;
            grid-template-columns: 70px 110px 1fr 350px;
            grid-template-rows: auto;
            align-items: start;
            border-bottom: 1px solid #e5e7eb;
            transition: background-color 0.2s ease;
            padding: 14px 10px;
            min-height: 80px;
        }

        .calendar-row.day-alt {
            background-color: #f9fafb;
        }

        .calendar-row.weekend {
            background-color: #fef3f2;
        }

        .calendar-row:hover {
            background-color: #f3f4f6;
        }

        /* =================================================================
           ČÍSLO DNE
           ================================================================= */
        .day-number {
            font-size: 32px;
            font-weight: 700;
            color: #1f2937;
            text-align: center;
            line-height: 1;
            padding-right: 10px;
            border-right: 2px solid #e5e7eb;
        }

        .date-note {
            font-size: 10px;
            color: #6b7280;
            margin-top: 4px;
            font-weight: 400;
            text-align: center;
        }

        /* =================================================================
           NÁZEV DNE
           ================================================================= */
        .day-name {
            font-size: 16px;
            font-weight: 600;
            color: #374151;
            padding: 0 12px;
            text-transform: capitalize;
            display: flex;
            align-items: center;
        }

        /* =================================================================
           SPECIÁLNÍ DNY - KONTEJNER
           ================================================================= */
        .day-events {
            display: flex;
            flex-direction: column;
            gap: 6px;
            padding-left: 20px;
            padding-right: 10px;
            grid-column: 4;
            justify-content: flex-start;
        }

        /* Jednotlivý speciální den */
        .day-event {
            font-size: 13px;
            line-height: 1.4;
            padding: 4px 8px;
            border-radius: 4px;
            display: inline-block;
            max-width: fit-content;
        }

        /* Skryje prázdné elementy */
        .day-event:empty {
            display: none;
        }

        /* =================================================================
           TYPY SPECIÁLNÍCH DNŮ - BAREVNÉ ROZLIŠENÍ
           ================================================================= */

        /* Státní svátky - červená */
        .event-holiday {
            background-color: #fee2e2;
            color: #991b1b;
            border-left: 3px solid #dc2626;
            font-weight: 600;
        }

        /* Školní prázdniny - oranžová */
        .event-school {
            background-color: #ffedd5;
            color: #9a3412;
            border-left: 3px solid #ea580c;
            font-weight: 600;
        }

        /* Významné dny - modrá */
        .event-special {
            background-color: #dbeafe;
            color: #1e40af;
            border-left: 3px solid #3b82f6;
        }

        /* Mezinárodní dny - zelená */
        .event-international {
            background-color: #d1fae5;
            color: #065f46;
            border-left: 3px solid #10b981;
        }

        /* Vlastní dny - fialová */
        .event-custom {
            background-color: #e9d5ff;
            color: #6b21a8;
            border-left: 3px solid #a855f7;
            font-weight: 600;
        }

        /* =================================================================
           TISK - OPTIMALIZACE PRO TISKÁRNU
           ================================================================= */
        @media print {
            body {
                background: white;
                padding: 0;
            }

            .calendar-container {
                box-shadow: none;
                border-radius: 0;
                max-width: 100%;
            }

            .calendar-header {
                height: 300px;
                page-break-after: avoid;
            }

            .month-image {
                /* Zajistí, že se obrázek vytiskne kompletně */
                width: 100% !important;
                height: 300px !important;
                object-fit: contain !important;
                print-color-adjust: exact;
                -webkit-print-color-adjust: exact;
            }

            .month-title {
                font-size: 56px;
                right: 40px;
            }

            .calendar-row {
                page-break-inside: avoid;
                min-height: 60px;
            }

            .calendar-row:hover {
                background-color: inherit;
            }

            /* Zachová barvy při tisku */
            .event-holiday,
            .event-school,
            .event-special,
            .event-international,
            .event-custom {
                print-color-adjust: exact;
                -webkit-print-color-adjust: exact;
            }
        }

        /* =================================================================
           RESPONZIVNÍ DESIGN - MOBILNÍ ZAŘÍZENÍ
           ================================================================= */
        @media (max-width: 1024px) {
            .calendar-body {
                grid-template-columns: 1fr;
            }

            .month-title {
                font-size: 48px;
                right: 30px;
            }
        }

        @media (max-width: 768px) {
            .calendar-header {
                height: 250px;
            }

            .month-title {
                font-size: 36px;
                right: 20px;
            }

            .calendar-row {
                grid-template-columns: 60px 90px 1fr 280px;
                padding: 10px 8px;
                min-height: 60px;
            }

            .day-number {
                font-size: 24px;
            }

            .day-name {
                font-size: 14px;
            }

            .day-event {
                font-size: 11px;
            }
        }

        @media (max-width: 480px) {
            body {
                padding: 0;
            }

            .calendar-container {
                border-radius: 0;
            }

            .calendar-header {
                height: 200px;
            }

            .month-title {
                font-size: 28px;
                right: 15px;
            }

            .calendar-row {
                grid-template-columns: 50px 80px 1fr 200px;
                gap: 8px;
            }
        }
    </style>
</head>
<body>
    <div class=""calendar-container"">
        <!-- Hlavička s obrázkem a názvem měsíce -->
        <div class=""calendar-header"">
            <div class=""image-section"">
                <img src=""@image-url"" class=""month-image"" alt=""Obrázek měsíce"" />
            </div>
            <div class=""month-title"">@month-name<br>@year</div>
        </div>

        <!-- Tělo kalendáře s dny -->
        <div class=""calendar-body"">
@foreach-begin
            <div class=""calendar-row @alt"">
                <!-- Číslo dne -->
                <div class=""day-number"">
                    @date
                    <div class=""date-note"">@date-note</div>
                </div>

                <!-- Název dne v týdnu -->
                <div class=""day-name"">@date-name</div>

                <!-- Speciální dny -->
                <div class=""day-events"">
                    <div class=""day-event event-holiday"">@date-holidays</div>
                    <div class=""day-event event-school"">@date-sholidays</div>
                    <div class=""day-event event-special"">@date-specialday</div>
                    <div class=""day-event event-international"">@date-interday</div>
                    <div class=""day-event event-custom"">@date-customday</div>
                </div>
            </div>
@foreach-end
        </div>
    </div>
</body>
</html>";
        }
    }


}

