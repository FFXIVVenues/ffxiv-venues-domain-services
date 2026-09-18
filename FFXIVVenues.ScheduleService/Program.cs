using FFXIVVenues.DomainData;
using FFXIVVenues.ScheduleService;
using FFXIVVenues.ScheduleService.Client.Events;
using FFXIVVenues.VenueService.Client.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Wolverine;
using Wolverine.RabbitMQ;
using Wolverine.Tracking;


var config = new ConfigurationBuilder()
    .AddEnvironmentVariables("FFXIV_VENUES_SCHEDULESERVICE__")
    .AddUserSecrets<Program>()
    .AddCommandLine(args)
    .Build();

var connectionString = config.GetConnectionString("FFXIVVenues") ?? throw new Exception("FFXIVVenues connection string not set");
var rabbitServiceUrl = config.GetValue<string>("Rabbit:ServiceUrl") ?? throw new Exception("Rabbit:ServiceUrl configuration not set");
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .WriteTo.Console()
    .Destructure.ByTransforming<VenueCreatedEvent>(v => new { v.VenueId })
    .Destructure.ByTransforming<VenueUpdatedEvent>(v => new { v.VenueId })
    .Destructure.ByTransforming<VenueDeletedEvent>(v => new { v.VenueId })
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddDomainData(connectionString);
builder.Services.AddSingleton<ScheduleExpander>();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();
builder.UseWolverine(opts =>
{
    opts.UseRabbitMq(rabbitServiceUrl)
        .DeclareExchange("FFXIVVenues.Venue.Events", e =>
            e.BindQueue("FFXIVVenues.ScheduleService.EventsInbox"))
        .AutoProvision();
    opts.ListenToRabbitQueue("FFXIVVenues.ScheduleService.EventsInbox");

    opts.PublishMessage<OpeningSoonEvent>()
        .ToRabbitExchange("FFXIVVenues.Openings.Events");
    opts.PublishMessage<ConfirmedOpeningSoonEvent>()
        .ToRabbitExchange("FFXIVVenues.Openings.Events");
});
builder.Services.AddHostedService<ScheduledExpansionService>();
builder.Services.AddHostedService<ScheduledOpeningSoonService>();


var host = builder.Build();

Log.Information("Starting migrations");
await host.Services.MigrateDomainDataAsync();
Log.Information("Migrations complete");

Log.Information("Starting host");
await host.RunAsync();