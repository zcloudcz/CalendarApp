using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarAppCore
{
    using System.Text.Json.Serialization;

    public class CalendarDay
    {
        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("dayNumber")]
        public string DayNumber { get; set; }

        [JsonPropertyName("dayInWeek")]
        public string DayInWeek { get; set; }

        [JsonPropertyName("monthNumber")]
        public string MonthNumber { get; set; }

        [JsonPropertyName("month")]
        public Month Month { get; set; }

        [JsonPropertyName("year")]
        public string Year { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("isHoliday")]
        public bool IsHoliday { get; set; }

        [JsonPropertyName("holidayName")]
        public string HolidayName { get; set; }
    }

    public class Month
    {
        [JsonPropertyName("nominative")]
        public string Nominative { get; set; }

        [JsonPropertyName("genitive")]
        public string Genitive { get; set; }
    }

}
