using FFXIVVenues.VenueModels;
using System;

namespace FFXIVVenues.Veni.Utils
{
    internal static class DayExtensions
    {

        public static Day Next(this Day day) =>
            day switch
            {
                Day.Monday => Day.Tuesday,
                Day.Tuesday => Day.Wednesday,
                Day.Wednesday => Day.Thursday,
                Day.Thursday => Day.Friday,
                Day.Friday => Day.Saturday,
                Day.Saturday => Day.Sunday,
                Day.Sunday => Day.Monday,
                _ => Day.Monday
            };

        public static string ToShortName(this Day day) =>
            day switch
            {
                Day.Monday => "Mon",
                Day.Tuesday => "Tue",
                Day.Wednesday => "Wed",
                Day.Thursday => "Thur",
                Day.Friday => "Fri",
                Day.Saturday => "Sat",
                Day.Sunday => "Sun",
                _ => "Mon"
            };

    }
}
