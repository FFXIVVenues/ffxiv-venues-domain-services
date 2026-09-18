using FFXIVVenues.DomainData.Context;
using FFXIVVenues.DomainData.Entities.Venues;
using FFXIVVenues.ScheduleService.Client.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Serilog;
using Wolverine;
using Timer = System.Timers.Timer;

internal class ScheduledOpeningSoonService(DomainDataContext db, IMessageBus bus) : IHostedService, IDisposable
{
    private Timer _timer = new Timer(TimeSpan.FromMinutes(5));

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this._timer.Elapsed += async (sender, e) => await RunSearch();
        this._timer.Start();
        _ = RunSearch();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this._timer.Stop();
        return Task.CompletedTask;
    }

    public async Task RunSearch()
    {
        Log.Information("Running scheduled broadcast for venues opening soon");
        var maxStart = DateTime.UtcNow.AddMinutes(30);
        var minEnd = DateTime.UtcNow.AddHours(1);
        var openings = await db.Openings
            .Where(o => o.Start <= maxStart 
                     && o.End >= minEnd 
                     && o.Status != OpeningStatus.OpeningCancelled
                     && o.Broadcasted == BroadcastStatus.NotBroadcasted)
            .ToListAsync();
        foreach (var opening in openings)
        {
            try
            {
                if (opening.Status == OpeningStatus.OpeningConfirmed)
                {
                    Log.Information("Broadcasting confirmed opening {opening} for venue {venue}", opening.Id, opening.VenueId);
                    await bus.PublishAsync(new ConfirmedOpeningSoonEvent(opening.VenueId, opening.Start, opening.End));
                    opening.Broadcasted = BroadcastStatus.BroadcastedConfirmed;
                }
                else
                {
                    Log.Information("Broadcasting unconfirmed opening {opening} for venue {venue}", opening.Id, opening.VenueId);
                    await bus.PublishAsync(new OpeningSoonEvent(opening.VenueId, opening.Start, opening.End));
                    opening.Broadcasted = BroadcastStatus.BroadcastedUnconfirmed;
                }
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to broadcast opening {opening} for venue {venue}", opening.Id, opening.VenueId);
            }
        }
        Log.Information("Completed scheduled broadcast for venues opening soon");
    }

    public void Dispose() =>
        this._timer?.Dispose();
}