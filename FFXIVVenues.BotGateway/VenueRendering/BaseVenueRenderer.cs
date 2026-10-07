using System;

namespace FFXIVVenues.BotGateway.VenueRendering;

public abstract class BaseVenueRenderer {
    
    protected static string Nth(int d)
    {
        if (d is > 3 and < 21) return "th";

        return (d % 10) switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };
    }

    protected static string TimeZone(string id) => id switch
    {
        "Eastern Standard Time" => "EST",
        "America/New_York" => "EST",
        "Central Standard Time" => "CST",
        "America/Chicago" => "CST",
        "Mountain Standard Time" => "MST",
        "America/Denver" => "MST",
        "Pacific Standard Time" => "PST",
        "America/Los_Angeles" => "PST",
        "Atlantic Standard Time" => "AST",
        "America/Halifax" => "AST",
        "Central Europe Standard Time" => "CEST",
        "Europe/Budapest" => "CEST",
        "E. Europe Standard Time" => "EEST",
        "Europe/Chisinau" => "EEST",
        "Greenwich Mean Time" => "GMT",
        "GMT Standard Time" => "GMT",
        "Europe/London" => "GMT",
        "UTC" => "Server Time",
        "Asia/Hong_Kong" => "HKT",
        "Australia/Perth" => "AWT",
        "Australia/Adelaide" => "ACT",
        "Australia/Sydney" => "AET",
        _ => id
    };
}


[Flags]
public enum VenueRenderFlags
{
    None = 0,
    FlagInvalidDiscord = 1,
    FlagInvalidSite = 2
}
