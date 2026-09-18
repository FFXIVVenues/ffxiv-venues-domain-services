using FFXIVVenues.DomainData.Context;
using FFXIVVenues.DomainData.Entities.Venues;
using FFXIVVenues.ScheduleService.ScheduleEnumeration;
using JasperFx.Events;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Text;

namespace FFXIVVenues.ScheduleService;

public class ScheduleExpander(DomainDataContext db)
{
    private const int DEFAULT_DAYS_TO_EXPAND = 30;

    public async Task<List<Opening>> ExpandVenueToOpeningsAsync(Venue venue, DateTimeOffset until = default)
    {
        if (until == default)
            until = DateTimeOffset.UtcNow.AddDays(DEFAULT_DAYS_TO_EXPAND);

        Log.Information("Expanding venue {venue} {venueName}", venue.Id, venue.Name);

        if (venue.Deleted.HasValue)
        {
            Log.Information("Venue {venue} is deleted, deleting any and all openings", venue.Id);
            await db.Openings.Where(v => v.VenueId == venue.Id).ExecuteDeleteAsync();
            return new List<Opening>();
        }

        var openings = ExpandScheduleToOpenings(venue, until);
        openings.AddRange(ExpandOpenOverridesToOpenings(venue, until));
        ApplyOverridesOnOpenings(venue, openings);
        openings = ConsolidateOverlappingOpenings(openings);

        var existingOpenings = db.Openings.Where(o => o.VenueId == venue.Id).ToList();
        var openingsToAdd = openings.Where(o => !existingOpenings.Any(eo => eo.GetHashCode() == o.GetHashCode()));
        var openingsToDelete = existingOpenings.Where(eo => !openings.Any(o => eo.GetHashCode() == o.GetHashCode())).Select(o => o.Id);

        await db.Openings.Where(o => o.VenueId == venue.Id && openingsToDelete.Contains(o.Id)).ExecuteDeleteAsync();
        await db.Openings.AddRangeAsync(openingsToAdd);
        await db.SaveChangesAsync();

        Log.Information("Expanded venue {venue} {venueName} out to {openings} openings", venue.Id, venue.Name, openings.Count);
        Log.Debug("Deleted {openings} and added {openings} openings for {venue}", openingsToDelete.Count(), openingsToAdd.Count(), venue.Id);

        return openings;
    }

    public List<Opening> ExpandScheduleToOpenings(Venue venue, DateTimeOffset until = default)
    {
        if (until == default)
            until = DateTimeOffset.UtcNow.AddDays(DEFAULT_DAYS_TO_EXPAND);

        var openings = new List<Opening>();
        foreach (var schedule in venue.Schedule)
        {
            var enumerator = new ScheduleEnumerator(schedule, DateTimeOffset.Now);
            while (enumerator.MoveNext())
            {
                if (enumerator.Current!.Start > until)
                    break;

                var opening = new Opening
                {
                    VenueId = venue.Id,
                    Start = enumerator.Current!.Start.ToUniversalTime(),
                    End = enumerator.Current!.End.ToUniversalTime(),
                    Status = OpeningStatus.OpeningUnconfirmed,
                    Source = OpeningSource.Schedule
                };
                openings.Add(opening);
            }
        }

        return openings;
    }

    public List<Opening> ExpandOpenOverridesToOpenings(Venue venue, DateTimeOffset until) =>
        venue.ScheduleOverrides.Where(o => o.Open && o.Start < until).Select(o => new Opening
        {
            VenueId = venue.Id,
            Start = o.Start.ToUniversalTime(),
            End = o.End.ToUniversalTime(),
            Status = OpeningStatus.OpeningUnconfirmed,
            Source = OpeningSource.Adhoc
        }).ToList();

    private static void ApplyOverridesOnOpenings(Venue venue, in List<Opening> openings)
    {
        foreach (var scheduleOverride in venue.ScheduleOverrides.Where(o => !o.Open))
        {
            var newOpeninigs = new List<Opening>();
            var toRemove = new List<Opening>();
            foreach (var opening in openings.ToList())
            {
                if (scheduleOverride.Start >= opening.Start
                    && scheduleOverride.Start < opening.End
                    && scheduleOverride.End <= opening.End)
                {
                    newOpeninigs.Add(new Opening
                    {
                        VenueId = venue.Id,
                        Start = scheduleOverride.Start.ToUniversalTime(),
                        End = opening.End.ToUniversalTime(),
                        Source = opening.Source,
                        Status = opening.Status,
                    });
                    opening.End = scheduleOverride.Start;
                    continue;
                }

                if (scheduleOverride.Start >= opening.Start
                    && scheduleOverride.Start < opening.End)
                {
                    opening.End = scheduleOverride.Start;
                    continue;
                }

                if (scheduleOverride.End > opening.Start
                    && scheduleOverride.End <= opening.End)
                {
                    opening.Start = scheduleOverride.End;
                    continue;
                }

                if (scheduleOverride.Start <= opening.Start
                    && scheduleOverride.End >= opening.End)
                {
                    toRemove.Add(opening);
                    continue;
                }
            }

            openings.RemoveAll(openings.Contains);
            openings.AddRange(newOpeninigs);
        }
    }

    private static List<Opening> ConsolidateOverlappingOpenings(List<Opening> openings)
    {
        openings = openings
                    .OrderBy(o => o.Start)
                    .Aggregate(new List<Opening>(), (acc, o) =>
                    {
                        if (acc.Count == 0)
                        {
                            acc.Add(o);
                            return acc;
                        }
                        var last = acc.Last();
                        if (last.End >= o.Start)
                        {
                            last.End = last.End > o.End ? last.End : o.End;
                            return acc;
                        }
                        acc.Add(o);
                        return acc;
                    });
        return openings;
    }

}
