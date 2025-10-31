
using System;

namespace CalendarAppCore;

internal class MonthCalendarDay
{
    public DateTime Date { get; }
    public bool IsAlt { get; set; }
    public bool IsWeekend { get; set; }
    
    public MonthCalendarDay(DateTime date)
    {
        Date = date;
        IsAlt = Date.Day % 2 == 0;
        IsWeekend = Date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
    }
}
