// See https://aka.ms/new-console-template for more information
using CalendarAppCore;
using System.Globalization;

var y = DateTime.Now.Year;
var m = DateTime.Now.Month;
var tmpl = "template.txt";
string img = null;

if (args.Length > 0)
{
    if(int.TryParse(args[0], out int _y))
        y = _y;

    if (int.TryParse(args[1], out var _m))
        m = _m;

    if (args.Length > 2)
        tmpl = args[2];

    if (args.Length > 3)
        img = args[3];
}

Console.WriteLine("Generuji kalendář...");
CultureInfo.CurrentCulture = new CultureInfo("cs-CZ");

var cl = new MonthCalendar(new() { Year = y, Month = m, ImagePath = img, TemplateFilePath = tmpl, RenderMode = MonthCalendarRenderMode.TwoRows, Region = "Blansko" });
await cl.ToFileAsync();

Console.WriteLine(cl.ExportFilePath);
Console.ReadLine();

//return;

for (var mm = 1; mm <= 12; mm++)
{
    var c = new MonthCalendar(new() { Year = 2025, Month = mm, RenderMode = MonthCalendarRenderMode.TwoRows, Region = "Blansko" });
    await c.ToFileAsync();
}