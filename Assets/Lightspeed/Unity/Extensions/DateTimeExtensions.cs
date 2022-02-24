using System;

namespace Rhinox.Lightspeed
{
    public static class DateTimeExtensions
    {
        public static DateTime FirstDayOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1);
        }

        public static int DaysInMonth(this DateTime value)
        {
            return DateTime.DaysInMonth(value.Year, value.Month);
        }

        public static DateTime LastDayOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.DaysInMonth());
        }

        public static DateTime SetTime(this DateTime value, DateTime time)
        {
            return new DateTime(value.Year, value.Month, value.Day, time.Hour, time.Minute, time.Second);
        }

        public static DateTime SetDate(this DateTime value, DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, value.Hour, value.Minute, value.Second);
        }
    }
}