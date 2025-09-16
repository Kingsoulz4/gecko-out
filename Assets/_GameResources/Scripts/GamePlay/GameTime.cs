using System;
using UnityEngine.Timeline;

public class GameTime : SingletonDontDestroyMono<GameTime>
{
    public long GetUtcTime()
    {
        return TicksToTimestamp(DateTime.UtcNow.Ticks);
    }

    public DateTime Now()
    {
        return TimestampToDateTime(GetUtcTime());
    }
    public bool IsNewDay(long timeLogin)
    {
        var d1 = TicksToDateTime(TimestampToTicks(timeLogin));
        DateTime d2 = TicksToDateTime(TimestampToTicks(GetUtcTime()));
        return d2.Date > d1.Date;
    }
    public bool IsNewWeek(long timeLogin)
    {
        var lastWeek = StartOfWeek(timeLogin / 86400000 * 86400000, DayOfWeek.Monday);
        var currentWeek = StartOfWeek(GetUtcTime() / 86400000 * 86400000, DayOfWeek.Monday);
        return currentWeek - lastWeek >= 604800000;
    }
    public bool IsNewMonth(long timeLogin)
    {
        var d1 = TicksToDateTime(TimestampToTicks(timeLogin));
        DateTime d2 = TicksToDateTime(TimestampToTicks(GetUtcTime()));
        return d2.Year > d1.Year || d2.Month > d1.Month;
    }
    public bool IsNewYear(long timeLogin)
    {
        var d1 = TicksToDateTime(TimestampToTicks(timeLogin));
        DateTime d2 = TicksToDateTime(TimestampToTicks(GetUtcTime()));
        return d2.Year > d1.Year;
    }

    #region Utils
    public static long TicksToTimestamp(long ticks)
    {
        return (ticks - 621355968000000000) / 10000;
    }
    public static long TimestampToTicks(long timstamp)
    {
        return (timstamp * 10000 + 621355968000000000);
    }
    public static DateTime TicksToDateTime(long ticks)
    {
        return new DateTime(ticks);
    }
    public static DateTime TimestampToDateTime(long timstamp)
    {
        return TicksToDateTime(TimestampToTicks(timstamp));
    }
    public static TimeSpan TicksToTimeSpan(long ticks)
    {
        return new TimeSpan(ticks);
    }
    public static TimeSpan MillisecondsToTimeSpan(long millisecond)
    {
        return new TimeSpan(millisecond * 10000);
    }
    public static long StartOfWeek(long dt, DayOfWeek startOfWeek)
    {
        int diff = (7 + (TimestampToWeekDays(dt) - startOfWeek)) % 7;
        return dt - diff * 86400000;
    }
    public static long StartOfMonth(long timestamp, int startOfMonth = 1)
    {
        var cur = TimestampToDateTime(timestamp);
        var d = new DateTime(cur.Year, cur.Month, startOfMonth);
        return TicksToTimestamp(d.Ticks);
    }
    public static long EndOfMonth(long timestamp)
    {
        var cur = TimestampToDateTime(timestamp);
        var d = new DateTime(cur.Year, cur.Month, DateTime.DaysInMonth(cur.Year, cur.Month));
        d = d.AddMilliseconds(86399999);
        return TicksToTimestamp(d.Ticks);
    }
    public static DayOfWeek TimestampToWeekDays(long timestamp)
    {
        return TicksToDateTime(TimestampToTicks(timestamp)).DayOfWeek;
    }
    #endregion
    #region UTC
    public static DateTime TimeSpanToDateTimeUTC(long timeSpan)
    {
        TimeZoneInfo localZone = TimeZoneInfo.Local;
        long offsetMilliseconds = (long)localZone.BaseUtcOffset.TotalMilliseconds;
        timeSpan += offsetMilliseconds;
        return TimestampToDateTime(timeSpan);
    }
    #endregion
}

