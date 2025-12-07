using FFXIVVenues.DomainData;
using FFXIVVenues.DomainData.Context;
using FFXIVVenues.DomainData.Mapping;
using FFXIVVenues.OGCard;
using Serilog;
using Serilog.Events;

var config = new ConfigurationBuilder()
    .AddEnvironmentVariables("FFXIV_VENUES_OGCARD__")
    .AddUserSecrets<Program>()
    .AddCommandLine(args)
    .Build();

var connectionString = config.GetConnectionString("FFXIVVenues");
var mediaUriTemplate = config.GetValue<string>("UriTemplate", 
    "https://images.ffxivvenues.dev/{venueId}/{bannerKey}");
var betterStackToken = config.GetValue<string>("Logging:BetterStackToken");
var minLevel = config.GetValue<LogEventLevel>("Logging:MinimumLevel");
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.BetterStack(betterStackToken)
    .MinimumLevel.Is(minLevel)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDomainData(connectionString, mediaUriTemplate);
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();
var app = builder.Build();

app.MapGet("/venue/{venueId}", (string venueId, IMapFactory mapFactory, DomainDataContext domainData) =>
{
    var query = domainData.Venues.AsQueryable().Where(v => v.Id == venueId);
    var venue = mapFactory.GetModelProjector().ProjectTo<FFXIVVenues.VenueModels.Venue>(query).SingleOrDefault();
    if (venue is null)
        return Results.NotFound();
    
    var template = new OGCardTemplate();
    template.Session = new Dictionary<string, object>();
    template.Session["venue"] = venue;
    return Results.Content(template.TransformText(), "text/html");
});

await app.RunAsync();