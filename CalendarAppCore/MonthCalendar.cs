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
            // 2. Template file path (TemplateFilePath) - for file system templates
            // 3. Built-in default template (GetDefaultTemplate) - fallback
            if (!string.IsNullOrEmpty(opts.TemplateContent))
            {
                _template = opts.TemplateContent;
            }
            else if (!string.IsNullOrEmpty(opts.TemplateFilePath) && File.Exists(opts.TemplateFilePath))
            {
                _template = File.ReadAllText(opts.TemplateFilePath);
            }
            else
            {
                _template = GetDefaultTemplate();
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

        private static string GetDefaultTemplate()
        {
            return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Monthly Calendar</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: Arial, sans-serif;
            background-color: #f5f5f5;
            padding: 20px;
        }

        .calendar-container {
            margin: 0 auto;
            background: white;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }

        .calendar-header {
            /*background: linear-gradient(135deg, #6B46C1, #8B5CF6);*/
            color: white;

            text-align: center;
            font-size: 24px;
            font-weight: bold;
        }

        .calendar-body {
			display: grid;
			grid-template-columns: repeat(2, 1fr);
            background: white;
        }

        .calendar-row {
            display: flex;
            align-items: center;
            border-bottom: 1px solid #e5e5e5;
            transition: background-color 0.2s ease;
        }

        .calendar-row.day-alt {
            background-color: #f8f4ff;
        }

        .calendar-row:nth-child(even) {
            /*background-color: #e9d5ff;*/
        }

        .calendar-row:hover {
            background-color: #ddd6fe;
        }

        .calendar-row:last-child {
            border-bottom: none;
        }

        .day-number {
            width: 60px;
            padding: 12px 16px;
            font-weight: bold;
            color: #4c1d95;
            text-align: center;
            border-right: 1px solid #e5e5e5;
        }

        .day-name {
            width: 80px;
            padding: 12px 16px;
            color: #6b7280;
            font-weight: 500;
            border-right: 1px solid #e5e5e5;
        }

        .day-event {
            flex: 1;
            padding: 12px 16px;
            color: #374151;
            font-weight: 500;
        }

        .holiday {
            color: #dc2626;
            font-weight: 600;
text-align: right;
        }

.month-title {
	font-size: 60px;
    position: absolute;
    top: 90px;
    right: 90px;
}

.weekend {
 background-color: #e9c5dd !important;
}

.date-note {
	font-size: 11px;
	color: #000;
}

        /* Responsive design */
        @media (max-width: 480px) {
            .calendar-container {
                max-width: 100%;
                margin: 0;
            }

            .day-number {
                width: 50px;
                padding: 10px 12px;
            }

            .day-name {
                width: 70px;
                padding: 10px 12px;
                font-size: 14px;
            }

            .day-event {
                padding: 10px 12px;
                font-size: 14px;
            }
        }
    </style>
</head>
<body>
    <div class=""calendar-container"">
        <div class=""calendar-header"">
<div class=""image-section"">
 <img src=""@image-url"" class=""month-image"" />
</div>
<div class=""month-title"">@month-name @year</div>
        </div>
        <div class=""calendar-body"">
@foreach-begin
            <div class=""calendar-row @alt"">
                <div class=""day-number"">@date
<div class=""date-note"">@date-note</div>
</div>
                <div class=""day-name"">@date-name</div>
                <div class=""day-event holiday"">@date-holidays</div>
                <div class=""day-event holiday"">@date-sholidays</div>
                <div class=""day-event holiday"">@date-specialday</div>
                <div class=""day-event holiday"">@date-interday</div>
            </div>
@foreach-end
        </div>
    </div>
</body>
</html>";
        }
    }


}

