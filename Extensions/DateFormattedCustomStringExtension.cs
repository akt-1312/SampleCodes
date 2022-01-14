using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.Extensions
{
    public static class DateFormattedCustomStringExtension
    {
        public static string FormattedDateString(this DateTime item)
        {
            var defaultDate = item;
            var day = defaultDate.Day.ToString();
            if (int.Parse(day) < 10)
            {
                day = "0" + day;
            }
            var month = defaultDate.Month.ToString();
            if (int.Parse(month) < 10)
            {
                month = "0" + month;
            }
            var year = defaultDate.Year;

            string result = month + "/" + day + "/" + year;
            return result;
        }
    }
}
