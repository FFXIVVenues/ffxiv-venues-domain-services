using System.Runtime.CompilerServices;
using FFXIVVenues.DomainData;
using FFXIVVenues.DomainData.Context;
using FFXIVVenues.DomainData.Mapping;
using FFXIVVenues.OGCardService;
using Serilog;
using Serilog.Events;

var config = new ConfigurationBuilder()
    .AddEnvironmentVariables("FFXIV_VENUES_OGCARD__")
    .AddUserSecrets<Program>()
    .AddCommandLine(args)
    .Build();

var connectionString = config.GetConnectionString("FFXIVVenues");
var redirectUriTemplate = config.GetValue<string>("RedirectUriTemplate", 
    "https://ffxivvenues.dev/venue/{venueId}");
var mediaUriTemplate = config.GetValue<string>("BannerUriTemplate", 
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

app.MapGet("/venue/{venueId}", (string venueId, IMapFactory mapFactory, DomainDataContext domainData, HttpContext context) =>
{
    context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
    context.Response.Headers.Pragma = "no-cache";
    context.Response.Headers.Expires = "0";
    
    var query = domainData.Venues.AsQueryable().Where(v => v.Id == venueId);
    var venue = mapFactory.GetModelProjector().ProjectTo<FFXIVVenues.VenueModels.Venue>(query).SingleOrDefault();
    if (venue is null)
        return Results.NotFound();
    
    var template = new OGCardTemplate();
    template.Session = new Dictionary<string, object>
    {
        ["venue"] = venue,
        ["redirect"] = redirectUriTemplate.Replace("{venueId}", venueId),
    };
        
    return Results.Content(template.TransformText(), "text/html", statusCode: 206);
});

await app.RunAsync();