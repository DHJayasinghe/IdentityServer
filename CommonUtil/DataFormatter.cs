using System;
using System.Globalization;

namespace CommonUtil
{
    /// <summary>
    /// Utility class to format data into another descriptive format
    /// </summary>
    public static class DataFormatter
    {
        /// <summary>
        /// Output formated datetime value, by comparing input datetime value and current datetime. 
        /// Returning formats are: HH:mm, Yesterday, Thursday or MM/dd/yy
        /// </summary>
        public static string FormatDateTime(DateTime dateTimeVal)
        {
            if (dateTimeVal.Date == DateTime.Now.Date)
                return dateTimeVal.ToString("HH:mm"); // 10:34 

            if (dateTimeVal.Date == DateTime.Now.AddDays(1).Date)
                return "Tomorrow";

            if (dateTimeVal.Date == DateTime.Now.AddDays(-1).Date)
                return "Yesterday";

            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;
            if (cal.GetWeekOfYear(dateTimeVal, dfi.CalendarWeekRule, dfi.FirstDayOfWeek) == cal.GetWeekOfYear(DateTime.Now, dfi.CalendarWeekRule, dfi.FirstDayOfWeek))
                return dateTimeVal.Date.ToString("dddd"); // Thursday

            return dateTimeVal.ToString("dd/MM/yy"); // 16/7/20
        }
    }
}
