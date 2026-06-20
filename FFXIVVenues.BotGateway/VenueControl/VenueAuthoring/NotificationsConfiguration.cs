using System.Linq;
using Discord;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring;

public class NotificationsConfiguration
{
    public NotificationConfiguration Approvals { get; set; } = new();
    public NotificationConfiguration MissingSplash { get; set; } = new();
    public ulong[] Help { get; set; } = [];
}

public class NotificationConfiguration
{
    public ulong[] Global { get; set; } = [];
    public ulong[] NorthAmerica { get; set; } = [];
    public ulong[] Oceania { get; set; } = [];
    public ulong[] Europe { get; set; } = [];
    public ulong[] Japan { get; set; } = [];

    public ulong[] ResolveFor(string region)
    {
        var regionRecipients = region switch
        {
            FfxivWorlds.REGION_NA  => this.NorthAmerica,  
            FfxivWorlds.REGION_EU  => this.Europe,  
            FfxivWorlds.REGION_OCE => this.Oceania,
            FfxivWorlds.REGION_JPN => this.Japan,
            _ => []
        };
        return this.Global.Union(regionRecipients).ToArray();
    }
}
