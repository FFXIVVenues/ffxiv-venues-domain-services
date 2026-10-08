using FFXIVVenues.DomainData;
using FFXIVVenues.DomainData.Context;
using FFXIVVenues.DomainData.Mapping;
using FFXIVVenues.OGCardService;
using Serilog;

var config = new ConfigurationBuilder()
    .AddEnvironmentVariables("FFXIV_VENUES_OGCARD__")
    .AddUserSecrets<Program>()
    .AddCommandLine(args)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .CreateLogger();

var connectionString = config.GetConnectionString("FFXIVVenues") ?? throw new Exception("FFXIVVenues connection string not set");
var mediaUriTemplate = config.GetValue<string>("MediaUriTemplate") ?? throw new Exception("MediaUriTemplate configuration not set");

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDomainData(c =>
{
    c.ConnectionString = connectionString;
    c.MediaUriTemplate = mediaUriTemplate;
});
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();
var app = builder.Build();

var redirectUriTemplate = config.GetValue<string>("RedirectUriTemplate", "https://ffxivvenues.dev/venue/{venueId}");
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