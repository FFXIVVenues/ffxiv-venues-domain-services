using FFXIVVenues.VenueModels;
using DtoDay = FFXIVVenues.VenueModels.Day;
using DomainDay = FFXIVVenues.DomainData.Entities.Venues.Day;

namespace FFXIVVenues.BotGateway.Utils;

internal static class DayExtensions
{

    public static DtoDay Next(this DtoDay day) =>
        day switch
        {
            DtoDay.Monday => DtoDay.Tuesday,
            DtoDay.Tuesday => DtoDay.Wednesday,
            DtoDay.Wednesday => DtoDay.Thursday,
            DtoDay.Thursday => DtoDay.Friday,
            DtoDay.Friday => DtoDay.Saturday,
            DtoDay.Saturday => DtoDay.Sunday,
            DtoDay.Sunday => DtoDay.Monday,
            _ => Day.Monday
        };
    
    public static DomainDay Next(this DomainDay day) =>
        day switch
        {
            DomainDay.Monday => DomainDay.Tuesday,
            DomainDay.Tuesday => DomainDay.Wednesday,
            DomainDay.Wednesday => DomainDay.Thursday,
            DomainDay.Thursday => DomainDay.Friday,
            DomainDay.Friday => DomainDay.Saturday,
            DomainDay.Saturday => DomainDay.Sunday,
            DomainDay.Sunday => DomainDay.Monday,
            _ => DomainDay.Monday
        };

    public static string ToShortName(this Day day) =>
        day switch
        {
            DtoDay.Monday => "Mon",
            DtoDay.Tuesday => "Tue",
            DtoDay.Wednesday => "Wed",
            DtoDay.Thursday => "Thur",
            DtoDay.Friday => "Fri",
            DtoDay.Saturday => "Sat",
            DtoDay.Sunday => "Sun",
            _ => "Mon"
        };
    
    public static string ToShortName(this DomainDay day) =>
        day switch
        {
            DomainDay.Monday => "Mon",
            DomainDay.Tuesday => "Tue",
            DomainDay.Wednesday => "Wed",
            DomainDay.Thursday => "Thur",
            DomainDay.Friday => "Fri",
            DomainDay.Saturday => "Sat",
            DomainDay.Sunday => "Sun",
            _ => "Mon"
        };
    

}
