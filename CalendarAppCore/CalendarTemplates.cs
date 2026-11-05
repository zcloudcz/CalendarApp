using System;
using System.Collections.Generic;
using System.Linq;

namespace CalendarAppCore
{
    /// <summary>
    /// Manages different calendar template variants with various designs.
    /// Provides access to built-in templates and their metadata.
    /// </summary>
    public static class CalendarTemplates
    {
        /// <summary>
        /// Gets all available template variants.
        /// </summary>
        public static List<TemplateInfo> GetAllTemplates()
        {
            return new List<TemplateInfo>
            {
                new TemplateInfo
                {
                    Id = "default",
                    Name = "Moderní (výchozí)",
                    Description = "Moderní design s gradientem, velkým obrázkem a barevným rozlišením speciálních dnů. Ideální pro tisk i zobrazení.",
                    PreviewImage = "/templates/preview-default.png",
                    Category = "modern"
                },
                new TemplateInfo
                {
                    Id = "minimal",
                    Name = "Minimalistická",
                    Description = "Čistý minimalistický design s černobílým schématem. Šetří toner při tisku.",
                    PreviewImage = "/templates/preview-minimal.png",
                    Category = "minimal"
                },
                new TemplateInfo
                {
                    Id = "colorful",
                    Name = "Barevná",
                    Description = "Živý design s výraznými barvami pro každý den v týdnu. Perfektní pro děti.",
                    PreviewImage = "/templates/preview-colorful.png",
                    Category = "colorful"
                },
                new TemplateInfo
                {
                    Id = "classic",
                    Name = "Klasická",
                    Description = "Tradiční design připomínající klasické papírové kalendáře.",
                    PreviewImage = "/templates/preview-classic.png",
                    Category = "classic"
                },
                new TemplateInfo
                {
                    Id = "compact",
                    Name = "Kompaktní",
                    Description = "Úsporný layout s menším písmem - více informací na jedné stránce.",
                    PreviewImage = "/templates/preview-compact.png",
                    Category = "compact"
                }
            };
        }

        /// <summary>
        /// Gets template content by ID.
        /// </summary>
        public static string GetTemplateById(string templateId)
        {
            return templateId?.ToLower() switch
            {
                "minimal" => GetMinimalTemplate(),
                "colorful" => GetColorfulTemplate(),
                "classic" => GetClassicTemplate(),
                "compact" => GetCompactTemplate(),
                _ => GetDefaultTemplate()
            };
        }

        /// <summary>
        /// Gets template info by ID.
        /// </summary>
        public static TemplateInfo GetTemplateInfo(string templateId)
        {
            return GetAllTemplates().FirstOrDefault(t => t.Id.Equals(templateId, StringComparison.OrdinalIgnoreCase));
        }

        // Původní moderní šablona (současný výchozí design)
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

        // Minimalistická šablona - černobílý design
        private static string GetMinimalTemplate()
        {
            return @"<!DOCTYPE html>
<html lang=""cs"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Kalendář @month-name @year</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Arial', 'Helvetica', sans-serif;
            background: #ffffff;
            padding: 20px;
        }

        .calendar-container {
            max-width: 1200px;
            margin: 0 auto;
            background: white;
            border: 2px solid #000;
        }

        .calendar-header {
            position: relative;
            width: 100%;
            height: 350px;
            overflow: hidden;
            background: #f5f5f5;
            border-bottom: 3px solid #000;
        }

        .month-image {
            width: 100%;
            height: 100%;
            object-fit: contain;
            object-position: center;
            display: block;
            filter: grayscale(100%);
        }

        .month-title {
            position: absolute;
            top: 30px;
            left: 30px;
            font-size: 60px;
            font-weight: 700;
            color: #000;
            text-transform: uppercase;
            letter-spacing: 2px;
            background: rgba(255, 255, 255, 0.9);
            padding: 10px 20px;
            border: 2px solid #000;
        }

        .calendar-body {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            background: white;
        }

        .calendar-row {
            display: grid;
            grid-template-columns: 60px 100px 1fr 300px;
            border-bottom: 1px solid #ddd;
            padding: 12px 10px;
            min-height: 70px;
        }

        .calendar-row.weekend {
            background-color: #f9f9f9;
        }

        .day-number {
            font-size: 28px;
            font-weight: 700;
            color: #000;
            text-align: center;
            border-right: 1px solid #ddd;
            padding-right: 10px;
        }

        .date-note {
            font-size: 9px;
            color: #666;
            margin-top: 4px;
        }

        .day-name {
            font-size: 14px;
            font-weight: 600;
            color: #000;
            padding: 0 12px;
            text-transform: uppercase;
            display: flex;
            align-items: center;
        }

        .day-events {
            display: flex;
            flex-direction: column;
            gap: 4px;
            padding-left: 20px;
            grid-column: 4;
        }

        .day-event {
            font-size: 12px;
            line-height: 1.3;
            padding: 3px 6px;
            border-left: 2px solid #000;
            color: #000;
        }

        .day-event:empty {
            display: none;
        }

        .event-holiday {
            font-weight: 700;
            border-left-width: 4px;
        }

        .event-school {
            font-weight: 600;
        }

        .event-custom {
            font-weight: 700;
            border-left-width: 3px;
        }

        @media print {
            body {
                padding: 0;
            }

            .calendar-container {
                border: 1px solid #000;
            }

            .month-image {
                filter: grayscale(100%);
                print-color-adjust: exact;
                -webkit-print-color-adjust: exact;
            }
        }
    </style>
</head>
<body>
    <div class=""calendar-container"">
        <div class=""calendar-header"">
            <img src=""@image-url"" class=""month-image"" alt=""Obrázek měsíce"" />
            <div class=""month-title"">@month-name @year</div>
        </div>
        <div class=""calendar-body"">
@foreach-begin
            <div class=""calendar-row @alt"">
                <div class=""day-number"">
                    @date
                    <div class=""date-note"">@date-note</div>
                </div>
                <div class=""day-name"">@date-name</div>
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

        // Barevná šablona - různé barvy pro dny v týdnu
        private static string GetColorfulTemplate()
        {
            return @"<!DOCTYPE html>
<html lang=""cs"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Kalendář @month-name @year</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Comic Sans MS', 'Arial Rounded MT Bold', cursive;
            background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
            padding: 20px;
        }

        .calendar-container {
            max-width: 1400px;
            margin: 0 auto;
            background: white;
            border-radius: 20px;
            overflow: hidden;
            box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
        }

        .calendar-header {
            position: relative;
            height: 400px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        }

        .month-image {
            width: 100%;
            height: 100%;
            object-fit: contain;
            display: block;
        }

        .month-title {
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            font-size: 80px;
            font-weight: 900;
            color: white;
            text-shadow: 4px 4px 0px #ff6b6b,
                         8px 8px 0px #4ecdc4;
            text-transform: uppercase;
        }

        .calendar-body {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            background: white;
        }

        .calendar-row {
            display: grid;
            grid-template-columns: 70px 110px 1fr 320px;
            padding: 14px 10px;
            min-height: 85px;
            border-bottom: 2px solid #f0f0f0;
        }

        /* Barevné pruhy pro dny v týdnu */
        .calendar-row:has(.day-name:contains('pondělí')) {
            border-left: 6px solid #ff6b6b;
        }
        .calendar-row:has(.day-name:contains('úterý')) {
            border-left: 6px solid #f9ca24;
        }
        .calendar-row:has(.day-name:contains('středa')) {
            border-left: 6px solid #6c5ce7;
        }
        .calendar-row:has(.day-name:contains('čtvrtek')) {
            border-left: 6px solid #00b894;
        }
        .calendar-row:has(.day-name:contains('pátek')) {
            border-left: 6px solid #fd79a8;
        }
        .calendar-row.weekend {
            background: linear-gradient(90deg, #fff5f5 0%, #ffffff 100%);
            border-left: 6px solid #e17055;
        }

        .day-number {
            font-size: 36px;
            font-weight: 900;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            text-align: center;
            padding-right: 10px;
        }

        .date-note {
            font-size: 10px;
            color: #666;
            margin-top: 4px;
        }

        .day-name {
            font-size: 16px;
            font-weight: 700;
            color: #2d3436;
            padding: 0 12px;
            text-transform: capitalize;
            display: flex;
            align-items: center;
        }

        .day-events {
            display: flex;
            flex-direction: column;
            gap: 6px;
            padding-left: 20px;
            grid-column: 4;
        }

        .day-event {
            font-size: 13px;
            padding: 5px 10px;
            border-radius: 20px;
            font-weight: 600;
        }

        .day-event:empty {
            display: none;
        }

        .event-holiday {
            background: linear-gradient(135deg, #ff6b6b 0%, #ee5a6f 100%);
            color: white;
        }

        .event-school {
            background: linear-gradient(135deg, #f9ca24 0%, #f0b429 100%);
            color: #2d3436;
        }

        .event-special {
            background: linear-gradient(135deg, #74b9ff 0%, #0984e3 100%);
            color: white;
        }

        .event-international {
            background: linear-gradient(135deg, #00b894 0%, #00cec9 100%);
            color: white;
        }

        .event-custom {
            background: linear-gradient(135deg, #fd79a8 0%, #e84393 100%);
            color: white;
        }

        @media print {
            body {
                background: white;
            }
            .event-holiday,
            .event-school,
            .event-special,
            .event-international,
            .event-custom {
                print-color-adjust: exact;
                -webkit-print-color-adjust: exact;
            }
        }
    </style>
</head>
<body>
    <div class=""calendar-container"">
        <div class=""calendar-header"">
            <img src=""@image-url"" class=""month-image"" alt=""Obrázek měsíce"" />
            <div class=""month-title"">@month-name<br>@year</div>
        </div>
        <div class=""calendar-body"">
@foreach-begin
            <div class=""calendar-row @alt"">
                <div class=""day-number"">
                    @date
                    <div class=""date-note"">@date-note</div>
                </div>
                <div class=""day-name"">@date-name</div>
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

        // Klasická šablona - tradiční design
        private static string GetClassicTemplate()
        {
            return @"<!DOCTYPE html>
<html lang=""cs"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Kalendář @month-name @year</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Georgia', 'Times New Roman', serif;
            background: #e8dcc4;
            padding: 30px;
        }

        .calendar-container {
            max-width: 1300px;
            margin: 0 auto;
            background: #fffef7;
            border: 3px solid #8b6914;
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
        }

        .calendar-header {
            position: relative;
            height: 380px;
            background: #f4ede4;
            border-bottom: 3px double #8b6914;
        }

        .month-image {
            width: 100%;
            height: 100%;
            object-fit: contain;
            display: block;
            border: 5px solid #fffef7;
        }

        .month-title {
            position: absolute;
            bottom: 20px;
            left: 50%;
            transform: translateX(-50%);
            font-size: 48px;
            font-weight: 700;
            color: #8b6914;
            text-transform: uppercase;
            background: rgba(255, 254, 247, 0.95);
            padding: 15px 40px;
            border: 2px solid #8b6914;
            border-radius: 5px;
        }

        .calendar-body {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            background: #fffef7;
        }

        .calendar-row {
            display: grid;
            grid-template-columns: 65px 105px 1fr 310px;
            padding: 12px 15px;
            min-height: 75px;
            border-bottom: 1px solid #d4c5a9;
        }

        .calendar-row.day-alt {
            background-color: #f9f6f0;
        }

        .calendar-row.weekend {
            background-color: #fef5e7;
        }

        .day-number {
            font-size: 30px;
            font-weight: 700;
            color: #8b6914;
            text-align: center;
            border-right: 2px solid #d4c5a9;
            padding-right: 10px;
        }

        .date-note {
            font-size: 9px;
            color: #8b6914;
            margin-top: 4px;
            font-style: italic;
        }

        .day-name {
            font-size: 15px;
            font-weight: 600;
            color: #5a4a1f;
            padding: 0 12px;
            text-transform: capitalize;
            display: flex;
            align-items: center;
            font-style: italic;
        }

        .day-events {
            display: flex;
            flex-direction: column;
            gap: 5px;
            padding-left: 20px;
            grid-column: 4;
        }

        .day-event {
            font-size: 12px;
            padding: 4px 10px;
            border-left: 3px solid #8b6914;
            background: #f9f6f0;
            color: #5a4a1f;
        }

        .day-event:empty {
            display: none;
        }

        .event-holiday {
            font-weight: 700;
            background: #fef5e7;
            border-left-color: #c27803;
        }

        .event-school {
            font-weight: 600;
            background: #fff9e6;
        }

        .event-custom {
            font-weight: 700;
            font-style: italic;
        }

        @media print {
            body {
                background: white;
            }
        }
    </style>
</head>
<body>
    <div class=""calendar-container"">
        <div class=""calendar-header"">
            <img src=""@image-url"" class=""month-image"" alt=""Obrázek měsíce"" />
            <div class=""month-title"">@month-name @year</div>
        </div>
        <div class=""calendar-body"">
@foreach-begin
            <div class=""calendar-row @alt"">
                <div class=""day-number"">
                    @date
                    <div class=""date-note"">@date-note</div>
                </div>
                <div class=""day-name"">@date-name</div>
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

        // Kompaktní šablona - více informací na stránce
        private static string GetCompactTemplate()
        {
            return @"<!DOCTYPE html>
<html lang=""cs"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Kalendář @month-name @year</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Arial', sans-serif;
            background: #f5f5f5;
            padding: 10px;
        }

        .calendar-container {
            max-width: 1600px;
            margin: 0 auto;
            background: white;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
        }

        .calendar-header {
            position: relative;
            height: 250px;
            background: #333;
        }

        .month-image {
            width: 100%;
            height: 100%;
            object-fit: contain;
            display: block;
        }

        .month-title {
            position: absolute;
            top: 15px;
            right: 20px;
            font-size: 40px;
            font-weight: 900;
            color: white;
            text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.8);
            text-transform: uppercase;
        }

        .calendar-body {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
        }

        .calendar-row {
            display: grid;
            grid-template-columns: 50px 90px 1fr 280px;
            padding: 8px 8px;
            min-height: 55px;
            border-bottom: 1px solid #e0e0e0;
        }

        .calendar-row.day-alt {
            background-color: #fafafa;
        }

        .calendar-row.weekend {
            background-color: #fff8f8;
        }

        .day-number {
            font-size: 22px;
            font-weight: 700;
            color: #333;
            text-align: center;
            border-right: 1px solid #e0e0e0;
            padding-right: 8px;
        }

        .date-note {
            font-size: 8px;
            color: #888;
            margin-top: 2px;
        }

        .day-name {
            font-size: 12px;
            font-weight: 600;
            color: #555;
            padding: 0 10px;
            text-transform: uppercase;
            display: flex;
            align-items: center;
        }

        .day-events {
            display: flex;
            flex-direction: column;
            gap: 3px;
            padding-left: 15px;
            grid-column: 4;
        }

        .day-event {
            font-size: 10px;
            padding: 2px 6px;
            border-left: 2px solid #666;
            line-height: 1.3;
        }

        .day-event:empty {
            display: none;
        }

        .event-holiday {
            background: #ffe0e0;
            color: #c00;
            font-weight: 600;
        }

        .event-school {
            background: #fff3e0;
            color: #e65100;
            font-weight: 600;
        }

        .event-special {
            background: #e3f2fd;
            color: #1565c0;
        }

        .event-international {
            background: #e8f5e9;
            color: #2e7d32;
        }

        .event-custom {
            background: #f3e5f5;
            color: #6a1b9a;
            font-weight: 600;
        }

        @media print {
            body {
                padding: 0;
            }
            .calendar-header {
                height: 200px;
            }
            .event-holiday,
            .event-school,
            .event-special,
            .event-international,
            .event-custom {
                print-color-adjust: exact;
                -webkit-print-color-adjust: exact;
            }
        }
    </style>
</head>
<body>
    <div class=""calendar-container"">
        <div class=""calendar-header"">
            <img src=""@image-url"" class=""month-image"" alt=""Obrázek měsíce"" />
            <div class=""month-title"">@month-name @year</div>
        </div>
        <div class=""calendar-body"">
@foreach-begin
            <div class=""calendar-row @alt"">
                <div class=""day-number"">
                    @date
                    <div class=""date-note"">@date-note</div>
                </div>
                <div class=""day-name"">@date-name</div>
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

    /// <summary>
    /// Metadata about a template variant.
    /// </summary>
    public class TemplateInfo
    {
        /// <summary>Unique identifier for the template</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Display name in Czech</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Description of the template</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Path to preview image</summary>
        public string PreviewImage { get; set; } = string.Empty;

        /// <summary>Category: modern, minimal, colorful, classic, compact</summary>
        public string Category { get; set; } = string.Empty;
    }
}
