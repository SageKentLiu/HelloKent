using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFACalendar
{
    public static class DateTimeHelper
    {
        public static DateTime GetEndOfMonth(int year, int month, int day)
        {
            DateTime dt = new DateTime(year, month, 1);
            dt.AddMonths(1).AddDays(-1);
            return dt;
        }

        public static DateTime GetEndOfMonth(int Year, int Month)
        {
            return new DateTime(Year, (int)Month, DateTime.DaysInMonth(Year, (int)Month));
        }

        public static DateTime GetEndOfMonth(DateTime dt)
        {
            dt.AddMonths(1).AddDays(-1);
            return dt;
        }

        public static DateTime GetNearDayOfWeek(DateTime dtmDate, ECALENDARCYCLE_DATEOFWEEK ePdDayWeekEnum)
        {
	        int iDayDiff = (short)((short)(ePdDayWeekEnum) - 1 - dtmDate.DayOfWeek);
            DateTime retDate;

	        if (Math.Abs(iDayDiff) < 4)
		        retDate = dtmDate.AddDays(+ iDayDiff);
	        else if (iDayDiff > 0)
		        retDate = dtmDate.AddDays(- (7 - iDayDiff));
	        else
		        retDate = dtmDate.AddDays(+ (7 + iDayDiff));

	        return retDate;
        }
    }
}
