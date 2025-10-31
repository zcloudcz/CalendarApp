using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CalendarAppCore;

/// <summary>
/// Represents a nameday (Czech "svátek") with associated statistics.
/// </summary>
public class Nameday
{
    /// <summary>
    /// The name being celebrated (e.g., "Jan", "Marie").
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The date when this name is celebrated (format: yyyy-MM-dd).
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Statistical count of how many people have this name in Czech Republic.
    /// </summary>
    public long Count { get; set; }
}

/// <summary>
/// Represents a holiday, including public holidays, school holidays, and special days.
/// </summary>
public class Holiday
{
    /// <summary>
    /// Name of the holiday (e.g., "Vánoce", "Velikonoce").
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Start date of the holiday (format: yyyy-MM-dd).
    /// For single-day holidays, Start and End are the same.
    /// </summary>
    public DateTime? Start { get; set; }

    /// <summary>
    /// End date of the holiday (format: yyyy-MM-dd).
    /// For single-day holidays, this equals Start.
    /// </summary>
    public DateTime? End { get; set; }

    /// <summary>
    /// Optional region identifier for school holidays (e.g., "Blansko", "Praha").
    /// Null or empty for nationwide holidays.
    /// </summary>
    public string Region { get; set; }
}

/// <summary>
/// Client for fetching calendar data from kurzy.cz website.
/// Provides methods to retrieve namedays, holidays, school holidays, and special days.
/// Implements caching by storing downloaded HTML files locally to reduce API calls.
/// </summary>
public class KurzyApiClient
{
    /// <summary>
    /// The year for which data should be fetched. Defaults to current year.
    /// </summary>
    private int _year;


    // URLs for fetching different types of calendar data from kurzy.cz
    private const string NameUrl = "https://www.kurzy.cz/kalendar/svatky/abecedni-seznam-jmen/";
    private string NamePath => $"abecedni-seznam-jmen{_year}.html"; // Local cache file path

    private const string SchoolHolidaysUrl = "https://www.kurzy.cz/kalendar/prazdniny/";
    private string SchoolHolidaysPath => $"prazdniny{_year}.html";

    private const string HolidaysUrl = "https://www.kurzy.cz/kalendar/statni-svatky/";
    private string HolidaysPath => $"statni_svatky{_year}.html";

    private const string SpecialDaysUrl = "https://www.kurzy.cz/kalendar/vyznamne-dny/";
    private string SpecialDaysPath => $"vyznamne_dny{_year}.html";

    private const string InternationalDaysUrl = "https://www.kurzy.cz/kalendar/mezinarodni-dny/";
    private string InternationalDaysPath => $"mezinarodni_dny{_year}.html";

    /// <summary>
    /// Initializes a new instance of the KurzyApiClient.
    /// </summary>
    /// <param name="year">The year for which to fetch data. If null, uses current year.</param>
    public KurzyApiClient(int? year = null)
    {
        _year = year ?? DateTime.Today.Year;
    }


    /// <summary>
    /// Fetches HTML content from the specified URL with local file caching.
    /// If the file already exists locally, reads from cache instead of downloading.
    /// This significantly reduces load on the external website and improves performance.
    /// </summary>
    /// <param name="url">The URL to fetch content from.</param>
    /// <param name="filePath">Local file path for caching the downloaded content.</param>
    /// <returns>The HTML content as a string.</returns>
    internal async Task<string> GetContentAsync(string url, string filePath)
    {
        string html;

        // Check if we have a cached version
        if (!File.Exists(filePath))
        {
            // No cache - download from web
            var httpClient = new HttpClient();
            html = await httpClient.GetStringAsync(url);

            // Save to cache for future use
            await File.WriteAllTextAsync(filePath, html);
        }
        else
        {
            // Use cached version
            html = await File.ReadAllTextAsync(filePath);
        }

        return html;
    }


    public async Task<List<Nameday>> GetNamesAsync()
    {
        int Year = DateTime.Now.Year;
        string html = await GetContentAsync(NameUrl, NamePath);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var table = doc.DocumentNode.SelectSingleNode("//table");
        var rows = table.SelectNodes(".//tr"); // přeskočí hlavičku

        var list = new List<Nameday>();

        foreach (var row in rows)
        {
            var cols = row.SelectNodes(".//td");
            if (cols == null || cols.Count < 2) continue;

            var name = cols[1].InnerText.Trim();
            var count = Regex.Replace(cols[2].InnerText, @"\s+", "");
            var dateRaw = cols[3].InnerText.Trim(); // např. "2.9."

            if (name.ToLower().StartsWith("ad")) { }

            var rec = new Nameday() { Name = name };

            if (long.TryParse(count, out var _c))
                rec.Count = _c;

            if (DateTime.TryParseExact(dateRaw, "d.M.", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                rec.Date = dt;
                list.Add(rec);
            }
        }

        return list;
    }

    public async Task<List<Holiday>> GetShoolHolidaysAsync()
    {
        var html = await GetContentAsync(SchoolHolidaysUrl, SchoolHolidaysPath);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var holidays = new List<Holiday>();

        // 1) Hledáme tabulku s třídou table-prazdniny
        var table = doc.DocumentNode.SelectSingleNode("//table[contains(@class,'table-prazdniny')]");
        if (table != null)
        {
            // vynecháme tbody, které jsou jen "blank" (oddělovače)
            var tbodies = table.SelectNodes("./tbody[not(contains(@class,'blank'))]");
            if (tbodies != null)
            {
                foreach (var tbody in tbodies)
                {
                    // první řádek obsahuje rozsah v <th>
                    var thNode = tbody.SelectSingleNode(".//tr[1]/th");
                    if (thNode == null) continue;

                    var dateRangeRaw = HtmlEntity.DeEntitize(thNode.InnerText).Trim();

                    // Najdeme všechny výskyty "d.M.yyyy" (mohou být mezery: "22. 12. 2025")
                    var dateMatches = Regex.Matches(dateRangeRaw, @"\d{1,2}\.\s*\d{1,2}\.\s*\d{4}");
                    string startDateStr = null, endDateStr = null;

                    if (dateMatches.Count >= 1)
                    {
                        startDateStr = NormalizeDateToken(dateMatches[0].Value);
                    }
                    if (dateMatches.Count >= 2)
                    {
                        endDateStr = NormalizeDateToken(dateMatches[1].Value);
                    }
                    else
                    {
                        // pokud není druhé datum, považujeme za jednorázové (start=end)
                        endDateStr = startDateStr;
                    }

                    var startIso = TryParseToIso(startDateStr);
                    var endIso = TryParseToIso(endDateStr);

                    // název je v druhém řádku v prvním td -> poslední <a> tam je plný název
                    var nameAnchor = tbody.SelectSingleNode(".//tr[2]/td[1]//a[last()]");
                    string name = nameAnchor != null
                        ? HtmlEntity.DeEntitize(nameAnchor.InnerText).Trim()
                        : (tbody.SelectSingleNode(".//tr[2]/td[1]")?.InnerText.Trim());

                    var regionTd = tbody.SelectSingleNode(".//tr[2]/td[2]");
                    string region = nameAnchor != null
                        ? HtmlEntity.DeEntitize(regionTd.InnerText).Trim()
                        : string.Empty;


                    if (!string.IsNullOrEmpty(name) && (startIso != null || endIso != null))
                    {
                        holidays.Add(new Holiday { Name = name, Start = startIso, End = endIso, Region = region });
                    }
                }
            }
        }

        // 2) Fallback: pokud nic nenajdeme, zkusíme skrytý block div.data .den
        if (holidays.Count == 0)
        {
            var denNodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'data')]//div[contains(@class,'den')]");
            if (denNodes != null)
            {
                foreach (var den in denNodes)
                {
                    var span = den.SelectSingleNode(".//span");
                    var dateRangeRaw = span != null ? HtmlEntity.DeEntitize(span.InnerText).Trim() : null;
                    if (string.IsNullOrEmpty(dateRangeRaw)) continue;

                    var dateMatches = Regex.Matches(dateRangeRaw, @"\d{1,2}\.\s*\d{1,2}\.\s*\d{4}");
                    string startDateStr = dateMatches.Count >= 1 ? NormalizeDateToken(dateMatches[0].Value) : null;
                    string endDateStr = dateMatches.Count >= 2 ? NormalizeDateToken(dateMatches[1].Value) : startDateStr;

                    var startIso = TryParseToIso(startDateStr);
                    var endIso = TryParseToIso(endDateStr);

                    var nameAnchor = den.SelectSingleNode(".//div[contains(@class,'U')]//a[last()]");
                    string name = nameAnchor != null ? HtmlEntity.DeEntitize(nameAnchor.InnerText).Trim() : null;

                    if (!string.IsNullOrEmpty(name) && (startIso != null || endIso != null))
                    {
                        holidays.Add(new Holiday { Name = name, Start = startIso, End = endIso });
                    }
                }
            }
        }

        return holidays;
    }
      
    public async Task<List<Holiday>> GetHolidaysAsync()
    {
        var html = await GetContentAsync(HolidaysUrl, HolidaysPath);
        
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var holidays = new List<Holiday>();

        // Najdeme všechny dny (div.den)
        var denNodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'den')]");
        if (denNodes == null)
        {
            throw new Exception("Nenalezeny žádné položky svátků.");
        }

        foreach (var den in denNodes)
        {
            // Datum je v <span>
            var span = den.SelectSingleNode(".//span");
            var spanText = span?.InnerText.Trim() ?? "";

            var dates = ExtractDates(spanText);
            if (dates.Count == 0)
            {
                // fallback: pokus o id dYYYYMMDD
                var id = den.GetAttributeValue("id", "");
                var m = Regex.Match(id, @"d(\d{8})");
                if (m.Success && DateTime.TryParseExact(m.Groups[1].Value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                {
                    dates.Add(dt);
                }
            }

            // Projdeme svátky v rámci dne
            var uNodes = den.SelectNodes(".//div[contains(@class,'U')]");
            if (uNodes == null) continue;

            foreach (var u in uNodes)
            {
                var aTags = u.SelectNodes(".//a");
                if (aTags == null || aTags.Count == 0) continue;

                var a = aTags.Last(); // poslední odkaz je plný název
                var name = HtmlEntity.DeEntitize(a.InnerText.Trim());
                //var href = a.GetAttributeValue("href", "");
                //if (href.StartsWith("/")) href = "https://www.kurzy.cz" + href;

                foreach (var d in dates)
                {
                    holidays.Add(new Holiday
                    {
                        Start = d,
                        Name = name,
                        //Url = href
                    });
                }
            }
        }

        return holidays;

    }


    public async Task<List<Holiday>> GetSpecialDaysAsync()
    {
        var result = new List<Holiday>();
        
        var content = await GetContentAsync(SpecialDaysUrl, SpecialDaysPath);

        var doc = new HtmlDocument();
        doc.LoadHtml(content);


        // najdi všechny divy s class="den"
        var dny = doc.DocumentNode.SelectNodes("//div[@class='den']");
        if (dny == null) return result;

        foreach (var den in dny)
        {
            var span = den.SelectSingleNode(".//span");
            var nazevNode = den.SelectSingleNode(".//a[not(@class='I bg')]");

            if (span != null && nazevNode != null)
            {
                string datumText = span.InnerText.Trim();
                string nazev = nazevNode.InnerText.Trim();

                // pokus o parsování datumu
                if (DateTime.TryParse(datumText, new System.Globalization.CultureInfo("cs-CZ"),
                                      System.Globalization.DateTimeStyles.None, out DateTime datum))
                {
                    result.Add(new Holiday
                    {
                        Start = datum,
                        Name = nazev
                    });
                }
                else
                {

                }
            }
        }

        return result;
    }


    public async Task<List<Holiday>> GetInternationalDaysAsync()
    {
        var result = new List<Holiday>();

        var html = await GetContentAsync(InternationalDaysUrl, InternationalDaysPath);
        
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var dny = doc.DocumentNode.SelectNodes("//div[@class='den']");
        if (dny == null) return result;

        foreach (var den in dny)
        {
            var span = den.SelectSingleNode(".//span");
            if (span == null) continue;

            string datumText = span.InnerText.Trim();
            if (!DateTime.TryParse(datumText,
                                   new System.Globalization.CultureInfo("cs-CZ"),
                                   System.Globalization.DateTimeStyles.None,
                                   out DateTime datum))
            {
                datum = DateTime.MinValue;
            }

            // projdi všechny odkazy, které nejsou ikona MD
            var nazvy = den.SelectNodes(".//div[@class='U']/a[not(@class='I bg')]");
            if (nazvy == null) continue;

            foreach (var a in nazvy)
            {
                result.Add(new Holiday
                {
                    Start = datum,
                    Name = a.InnerText.Trim()
                });
            }
        }

        return result;
    }



    internal string NormalizeDateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        // odstraní mezery: "22. 12. 2025" -> "22.12.2025"
        return Regex.Replace(token, @"\s+", "");
    }

    internal DateTime? TryParseToIso(string d)
    {
        if (string.IsNullOrEmpty(d)) return null;
        if (DateTime.TryParseExact(d, "d.M.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return dt;
        return null;
    }




    /// <summary>
    /// Extrahuje datumy z textu typu "Středa 1. ledna 2025"
    /// </summary>
    List<DateTime> ExtractDates(string text)
    {
        var result = new List<DateTime>();
        if (string.IsNullOrWhiteSpace(text)) return result;

        // odstraníme více mezer
        var clean = Regex.Replace(text, @"\s+", " ").Trim();

        // match např. "1. ledna 2025"
        var match = Regex.Match(clean, @"(\d{1,2})\.\s*([A-Za-zÁ-ž]+)\s+(\d{4})");
        if (match.Success)
        {
            if (int.TryParse(match.Groups[1].Value, out int day) &&
                TryParseCzechMonth(match.Groups[2].Value, out int month) &&
                int.TryParse(match.Groups[3].Value, out int year))
            {
                try { result.Add(new DateTime(year, month, day)); } catch { }
            }
        }

        return result;
    }

    bool TryParseCzechMonth(string token, out int month)
    {
        month = 0;
        token = token.RemoveDiacritics().ToLowerInvariant();
        var map = new Dictionary<string, int>
        {
            {"ledna", 1}, {"unora", 2}, {"brezna", 3}, {"dubna", 4},
            {"kvetna", 5}, {"cervna", 6}, {"cervence", 7}, {"srpna", 8},
            {"zari", 9}, {"rijna", 10}, {"listopadu", 11}, {"prosince", 12}
        };
        return map.TryGetValue(token, out month);
    }

    /// <summary>
    /// Retrieves a list of all unique regions from school holidays data.
    /// Useful for populating region dropdowns in the UI.
    /// </summary>
    /// <returns>A sorted list of distinct region names (e.g., "Blansko", "Praha").</returns>
    public async Task<List<string>> GetRegionsAsync()
    {
        // Fetch all school holidays which contain region information
        var holidays = await GetShoolHolidaysAsync();

        // Extract unique regions, filter out empty values, and sort alphabetically
        var regions = holidays
            .Where(h => !string.IsNullOrWhiteSpace(h.Region))
            .Select(h => h.Region.Trim())
            .Distinct()
            .OrderBy(r => r)
            .ToList();

        return regions;
    }
}

