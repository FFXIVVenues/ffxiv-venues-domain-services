using FFXIVVenues.DomainData;
using FFXIVVenues.DomainSecurity;
using FFXIVVenues.VenueService.Client.Events;
using FFXIVVenues.WebHookService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly.Timeout;
using Serilog;
using Wolverine;
using Wolverine.RabbitMQ;

var config = new ConfigurationBuilder()
    .AddEnvironmentVariables("FFXIV_VENUES_WEBHOOKSERVICE__")
    .AddUserSecrets<Program>()
    .AddCommandLine(args)
    .Build();

var webHooks = new List<WebHook>();
config.GetSection("WebHooks").Bind(webHooks);
var connectionString = config.GetConnectionString("FFXIVVenues") ?? throw new Exception("FFXIVVenues connection string not set");
var bannerUriTemplate = config.GetValue<string>("BannerUriTemplate") ?? throw new Exception("BannerUriTemplate configuration not set");
var rabbitServiceUrl = config.GetValue<string>("Rabbit:ServiceUrl") ?? throw new Exception("Rabbit:ServiceUrl configuration not set");
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .WriteTo.Console()
    .Destructure.ByTransforming<FFXIVVenues.VenueModels.Venue>(
        v => new { VenueId = v.Id, VenueName = v.Name })
    .Destructure.ByTransforming<FFXIVVenues.DomainData.Entities.Venues.Venue>(
        v => new { VenueId = v.Id, VenueName = v.Name })
    .Destructure.ByTransforming<VenueCreatedEvent>(v => new { v.VenueId })
    .Destructure.ByTransforming<VenueUpdatedEvent>(v => new { v.VenueId })
    .Destructure.ByTransforming<VenueDeletedEvent>(v => new { v.VenueId })
    .Destructure.ByTransforming<WebHook>(v => new { v.Name })
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton(config);
builder.Services.AddSingleton<IEnumerable<WebHook>>(webHooks);
builder.Services.AddSingleton<WebHookEventDistributor>();
builder.Services.AddDomainData(c =>
{
    c.ConnectionString = connectionString;
    c.MediaUriTemplate = bannerUriTemplate;
});
builder.Services.AddSecurityServices(o =>
{
    o.SigningPrivateKeyPath = config.GetValue<string>("Security:Signing:Ed25519:PrivateKeyPath") ?? o.SigningPrivateKeyPath;
    o.SigningPublicKeyPath = config.GetValue<string>("Security:Signing:Ed25519:PublicKeyPath") ?? o.SigningPublicKeyPath;
});
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();
builder.UseWolverine(opts =>
{
    opts.UseRabbitMq(rabbitServiceUrl)
        .DeclareExchange("FFXIVVenues.Venue.Events", e =>
            e.BindQueue("FFXIVVenues.WebHookService.EventsInbox"))
        .AutoProvision();
    opts.ListenToRabbitQueue("FFXIVVenues.WebHookService.EventsInbox");
});
foreach (var webHook in webHooks)
    builder.Services.AddHttpClient($"webhook-{webHook.Name}",
        c => c.Timeout = Timeout.InfiniteTimeSpan)
        .SetHandlerLifetime(Timeout.InfiniteTimeSpan)
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2)   // max time a failover IP can be stale
        })
        .AddPolicyHandler(WebHookRetryPolicy.Create())
        .AddPolicyHandler(TimeoutPolicy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30)));

var host = builder.Build();

Log.Information("Starting migrations");
await host.Services.MigrateDomainDataAsync();
Log.Information("Migrations complete");

Log.Information("Starting host");
await host.RunAsync();