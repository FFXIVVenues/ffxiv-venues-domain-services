using FFXIVVenues.VenueModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace FFXIVVenues.Veni;

internal static partial class Bootstrap
{
    internal static void ConfigureLogging(HostApplicationBuilder hostBuilder, Configurations config)
    {
        hostBuilder.Logging.ClearProviders();
        hostBuilder.Logging.AddSerilog();
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.BetterStack(config.LoggingConfig.BetterStackToken)
            .MinimumLevel.Is(config.LoggingConfig.MinimumLevel)
            .Destructure.ByTransforming<Venue>(
                v => new { VenueId = v.Id, VenueName = v.Name })
            .CreateLogger();
        hostBuilder.Services.AddSingleton(Log.Logger);
    }
}

internal class LoggingConfiguration
{
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Debug;
    public string BetterStackToken { get; set; }
}