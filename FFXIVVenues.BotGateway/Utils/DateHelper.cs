using System;
using System.Collections.Generic;

namespace FFXIVVenues.Veni.Utils;

public static class DateHelper
{
    public static IEnumerable<DateTimeOffset> GetNextNDates(int n, string timeZoneId)
    {
        var timezone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var now = DateTime.UtcNow;
        var offset = timezone.GetUtcOffset(now);
        now = now.Add(offset);
        var current = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, offset);

        yield return current; 
        for (var i = 0; i < n; i++)
        {
            current = current.AddDays(1);
            yield return current;
        }
    }
    
    public static DateTimeOffset[] GetNextNDatesForDay(int n, DayOfWeek day, string timeZoneId)
    {
        var timezone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var dates = new DateTimeOffset[n];
        var now = DateTime.UtcNow;
        var todayInZone = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, timezone.GetUtcOffset(now));

        for (int i = 0; i < n; i++)
        {
            var daysUntilNextDay = ((int)day - (int)todayInZone.DayOfWeek + 7) % 7;
            dates[i] = todayInZone.AddDays(daysUntilNextDay);
            todayInZone = dates[i].AddDays(1);
        }

        return dates;
    }
}