using FFXIVVenues.DomainData.Entities.Venues;
using System;
using System.Collections.Generic;
using System.Text;

namespace FFXIVVenues.ScheduleService.ScheduleEnumeration;

public static class DayExtensions
{
    public static Day Next(this Day day, int numberOfDays = 1) =>
        Scrub(day, numberOfDays);

    public static Day Previous(this Day day, int numberOfDays = 1) =>
        Scrub(day, 7 - numberOfDays);

    public static Day Scrub(this Day day, int scrub = 1) =>
        (Day)(((int)day + scrub) % 7);

    public static DayOfWeek ToDayOfWeek(this Day day) =>
        (DayOfWeek)((int)(day + 1) % 7);

}