using System.Collections.ObjectModel;
using TimeZoneMap = (string TimeZoneKey, string TimeZoneLabel);

namespace FFXIVVenues.Veni.Utils;

public static class TimeZones
{
    // ReSharper disable once CollectionNeverUpdated.Global
    public static readonly ReadOnlyCollection<TimeZoneMap> SupportedTimeZones = new ([
        ("America/New_York", "Eastern Standard Time (EST)" ),
        ( "America/Chicago", "Central Standard Time (CST)" ),
        ( "America/Denver", "Mountain Standard Time (MST)" ),
        ( "America/Los_Angeles", "Pacific Standard Time (PST)" ),
        ( "America/Halifax", "Atlantic Standard Time (AST)" ),
        ( "UTC", "Server Time (UTC)" ),
        ( "Europe/London", "Greenwich Mean Time (GMT)" ),
        ( "Europe/Budapest", "Central European Time (CEST)" ),
        ( "Europe/Chisinau", "Eastern European Time (EEST)" ),
        ( "Asia/Hong_Kong", "Hong Kong Time (HKT)" ),
        ( "Australia/Perth", "Australian Western Time (AWST)" ),
        ( "Australia/Adelaide", "Australian Central Time (ACST)" ),
        ( "Australia/Sydney", "Australian Eastern Time (AEST)" )
    ]);
}