using FFXIVVenues.DomainData;
using FFXIVVenues.FlagService.Client;
using FFXIVVenues.FlagService.Client.Commands;
using Serilog;
using Serilog.Events;
using Wolverine;
using Wolverine.RabbitMQ;

var config = new ConfigurationBuilder()
    .AddEnvironmentVariables("FFXIV_VENUES_FLAGSERVICE__")
    .AddUserSecrets<Program>()
    .AddCommandLine(args)
    .Build();

var connectionString = config.GetConnectionString("FFXIVVenues");
var mediaUriTemplate = config.GetValue<string>("MediaStorage:UriTemplate");
var betterStackToken = config.GetValue<string>("Logging:BetterStackToken");
var minLevel = config.GetValue<LogEventLevel>("Logging:MinimumLevel");
var rabbitServiceUrl = config.GetValue<string>("Rabbit:ServiceUrl");
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.BetterStack(betterStackToken)
    .MinimumLevel.Is(minLevel)
    .Destructure.ByTransforming<FFXIVVenues.VenueModels.Venue>(
        v => new { VenueId = v.Id, VenueName = v.Name })
    .Destructure.ByTransforming<FFXIVVenues.DomainData.Entities.Venues.Venue>(
        v => new { VenueId = v.Id, VenueName = v.Name })
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddDomainData(connectionString, mediaUriTemplate);
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();
builder.UseWolverine(opts =>
{
    opts.UseRabbitMq(rabbitServiceUrl).AutoProvision();
    opts.AddFlagServiceMessages();
    opts.ListenToRabbitQueue("FFXIVVenues.Flagging.Commands")
        .ProcessInline();
});


var host = builder.Build();

await host.Services.MigrateDomainDataAsync();
await host.RunAsync();