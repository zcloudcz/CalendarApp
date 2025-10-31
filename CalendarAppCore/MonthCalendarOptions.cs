using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarAppCore
{
    /// <summary>
    /// Configuration options for generating a month calendar.
    /// Includes settings for year, month, template, image, rendering mode, and region.
    /// </summary>
    public class MonthCalendarOptions
    {
        private string _imagePath;

        /// <summary>
        /// Initializes default options: current year/month, default template, and season-appropriate image.
        /// </summary>
        public MonthCalendarOptions()
        {
            Year = DateTime.Today.Year;
            Month = DateTime.Today.Month;
            TemplateFilePath = "template.txt";
            _imagePath = GetDefaultImageUrl(Month);  // Sets seasonal image based on month
            RenderMode = MonthCalendarRenderMode.Default;
        }

        /// <summary>
        /// Returns a season-appropriate default image URL from Unsplash.
        /// Images are pre-sized to 1800x400px (panoramic format).
        /// </summary>
        /// <param name="month">Month number (1-12)</param>
        /// <returns>Unsplash image URL for the corresponding season</returns>
        private string GetDefaultImageUrl(int month)
        {
            return month switch
            {
                12 or 1 or 2 => "https://images.unsplash.com/photo-1548625361-1def2bd128db?w=1800&h=400&fit=crop", // Winter
                3 or 4 or 5 => "https://images.unsplash.com/photo-1490750967868-88aa4486c946?w=1800&h=400&fit=crop", // Spring
                6 or 7 or 8 => "https://images.unsplash.com/photo-1506905925346-21bda4d32df4?w=1800&h=400&fit=crop", // Summer
                _ => "https://images.unsplash.com/photo-1441974231531-c6227db76b6e?w=1800&h=400&fit=crop" // Fall (9, 10, 11)
            };
        }

        /// <summary>Year for which to generate the calendar (e.g., 2024)</summary>
        public int Year { get; set; }

        /// <summary>Month for which to generate the calendar (1-12)</summary>
        public int Month { get; set; }

        /// <summary>File system path to the HTML template file (for console app usage)</summary>
        public string TemplateFilePath { get; set; }

        /// <summary>Direct template content as a string (for web app usage - allows user uploads)</summary>
        public string TemplateContent { get; set; }

        /// <summary>
        /// URL or file path to the header image.
        /// Recommended size: 1800×400px (panoramic format, aspect ratio 4.5:1)
        /// </summary>
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _imagePath = value;
            }
        }

        /// <summary>
        /// Render mode for calendar layout.
        /// Default: Standard single-column layout
        /// TwoRows: Two-column layout (1-15 and 16-31 side by side)
        /// </summary>
        public MonthCalendarRenderMode RenderMode { get; set; }

        /// <summary>
        /// Optional region filter for school holidays (e.g., "Blansko", "Praha").
        /// If set, only shows school holidays for this specific region.
        /// </summary>
        public string Region { get; set; }
    }

    /// <summary>
    /// Defines how the calendar days should be rendered in the HTML output.
    /// </summary>
    public enum MonthCalendarRenderMode
    {
        /// <summary>Standard sequential layout (day 1-31 in order)</summary>
        Default = 0,

        /// <summary>Two-column layout: left column (1-15), right column (16-31)</summary>
        TwoRows = 1,
    }
}
