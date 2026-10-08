using FFXIVVenues.DomainData;
using FFXIVVenues.FlagService.Client;
using FFXIVVenues.FlagService.Client.Events;
using Serilog;
using Wolverine;
using Wolverine.RabbitMQ;

var config = new ConfigurationBuilder()
    .AddEnvironmentVariables("FFXIV_VENUES_FLAGSERVICE__")
    .AddUserSecrets<Program>()
    .AddCommandLine(args)
    .Build();

var connectionString = config.GetConnectionString("FFXIVVenues") ?? throw new Exception("FFXIVVenues connection string not set");
var mediaUriTemplate = config.GetValue<string>("MediaUriTemplate") ?? throw new Exception("MediaUriTemplate configuration not set");
var rabbitServiceUrl = config.GetValue<string>("Rabbit:ServiceUrl") ?? throw new Exception("Rabbit:ServiceUrl configuration not set");

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .WriteTo.Console()
    .Destructure.ByTransforming<FFXIVVenues.VenueModels.Venue>(
        v => new { VenueId = v.Id, VenueName = v.Name })
    .Destructure.ByTransforming<FFXIVVenues.DomainData.Entities.Venues.Venue>(
        v => new { VenueId = v.Id, VenueName = v.Name })
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddDomainData(c =>
{
    c.ConnectionString = connectionString;
    c.MediaUriTemplate = mediaUriTemplate;
});
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();
builder.UseWolverine(opts =>
{
    opts.UseRabbitMq(rabbitServiceUrl).AutoProvision();
    opts.AddFlagServiceMessages();
    opts.ListenToRabbitQueue("FFXIVVenues.Flagging.Commands")
        .ProcessInline();
    opts.PublishMessage<VenueFlaggedEvent>()
        .ToRabbitExchange("FFXIVVenues.Flagging.Events");
    opts.PublishMessage<FlagResolvedEvent>()
        .ToRabbitExchange("FFXIVVenues.Flagging.Events");
    opts.PublishMessage<FlagDismissedEvent>()
        .ToRabbitExchange("FFXIVVenues.Flagging.Events");
});


var host = builder.Build();

Log.Information("Starting migrations");
await host.Services.MigrateDomainDataAsync();
Log.Information("Migrations complete");

Log.Information("Starting host");
await host.RunAsync();