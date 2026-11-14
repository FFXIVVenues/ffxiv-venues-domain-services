using FFXIVVenues.DomainData;
using FFXIVVenues.FlagService.Client;
using FFXIVVenues.FlagService.Client.Events;
using JasperFx.CodeGeneration.Model;
using Serilog;
using Wolverine;
using Wolverine.RabbitMQ;

var config = new ConfigurationBuilder()
    .AddEnvironmentVariables("FFXIV_VENUES_FLAGSERVICE__")
    .AddUserSecrets<Program>()
    .AddCommandLine(args)
    .Build();

var connectionString = config.GetConnectionString("FFXIVVenues");
var mediaUriTemplate = config.GetValue<string>("MediaStorage:UriTemplate");
var rabbitServiceUrl = config.GetValue<string>("Rabbit:ServiceUrl");
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .WriteTo.Console()
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
    opts.ServiceLocationPolicy = ServiceLocationPolicy.AllowedButWarn;
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