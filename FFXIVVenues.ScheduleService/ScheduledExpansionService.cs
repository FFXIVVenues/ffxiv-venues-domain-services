using FFXIVVenues.DomainData.Context;
using FFXIVVenues.ScheduleService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Timers;
using Timer = System.Timers.Timer;

internal class ScheduledExpansionService(DomainDataContext db, ScheduleExpander expander, IServiceProvider serviceProvider) : IHostedService, IDisposable
{
    private Timer _timer = new Timer(TimeSpan.FromHours(1));

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this._timer.Elapsed += async (sender, e) => await RunExpansion();
        this._timer.Start();
        _ = RunExpansion();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this._timer.Stop();
        return Task.CompletedTask;
    }

    public async Task RunExpansion()
    {
        Log.Information("Running scheduled expansion of all venues");
        foreach (var venue in await db.Venues.ToListAsync())
            try
            {
                await expander.ExpandVenueToOpeningsAsync(venue);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to expand {venue} {venueName}", venue.Id, venue.Name);
            }
        Log.Information("Completed scheduled expansion of all venues");
    }

    public void Dispose() =>
        this._timer?.Dispose();
}