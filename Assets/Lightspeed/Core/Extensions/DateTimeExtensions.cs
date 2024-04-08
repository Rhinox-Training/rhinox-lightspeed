using System;

namespace Rhinox.Lightspeed
{
    public static class DateTimeExtensions
    {
        public const int HOURS_IN_DAY = 24;
        public const int MINUTES_IN_DAY = 24 * 60;
        public const int SECONDS_IN_DAY = 24 * 60 * 60;
        
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

        public static int SecondsPassedThatDay(this DateTime dateTime)
        {
            return Convert.ToInt32(dateTime.TimeOfDay.TotalSeconds);
        }

        public static DateTime Lerp(this DateTime dtA, DateTime dtB, float t)
        {
            return dtA.AddTicks((long)((dtB.Ticks - dtA.Ticks) * t));
        }
    }
}