using FFXIVVenues.ApiGateway.Bootstrap;
using FFXIVVenues.ApiGateway.Controllers.OData;
using FFXIVVenues.ApiGateway.Helpers;
using FFXIVVenues.ApiGateway.Media;
using FFXIVVenues.ApiGateway.Security;
using FFXIVVenues.DomainData;
using FFXIVVenues.DomainSecurity;
using FFXIVVenues.FlagService.Client;
using FFXIVVenues.VenueService.Client.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OData.ModelBuilder;
using Scalar.AspNetCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using FFXIVVenues.ApiGateway.Helpers.Edm;
using FFXIVVenues.ApiGateway.Security.V1._0;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Wolverine;
using Wolverine.RabbitMQ;

var environment = args.SkipWhile(s => !string.Equals(s, "--environment", StringComparison.OrdinalIgnoreCase)).Skip(1).FirstOrDefault()
                  ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                  ?? Environments.Production;

var config = new ConfigurationBuilder()
    .AddEnvironmentVariables("FFXIV_VENUES_API:")
    .AddUserSecrets<Program>()
    .AddCommandLine(args)
    .Build();

var connectionString = config.GetConnectionString("FFXIVVenues") ?? throw new Exception("Configuration ConnectionStrings:FFXIVVenues not set");
var mediaUriTemplate = config.GetValue<string>("MediaStorage:UriTemplate") ?? throw new Exception("Configuration MediaStorage:UriTemplate not set");
var rabbitServiceUrl = config.GetValue<string>("Rabbit:ServiceUrl") ?? throw new Exception("Configuration Rabbit:ServiceUrl not set");
var authorisationKeys = config.GetSection("Security:AuthorizationKeys").Get<List<AuthorizationKey>>() ?? throw new Exception("Configuration Security:AuthorizationKeys not set");
var discordBotToken = config.GetValue<string>("DiscordBotToken") ?? throw new Exception("Configuration DiscordBotToken not set");
if (authorisationKeys.Count == 0) throw new Exception("Configuration Security:AuthorizationKeys empty");

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .WriteTo.Console()
    .Destructure.ByTransforming<FFXIVVenues.VenueModels.Venue>(
        v => new { VenueId = v.Id, VenueName = v.Name })
    .Destructure.ByTransforming<FFXIVVenues.DomainData.Entities.Venues.Venue>(
        v => new { VenueId = v.Id, VenueName = v.Name })
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Environment.EnvironmentName = environment;
builder.Configuration.AddConfiguration(config);
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();
builder.Host.UseWolverine(opts =>
{
    opts.UseRabbitMq(rabbitServiceUrl)
        .AutoProvision();
    opts.AddFlagServiceMessages();
    // Move to Venue Service soon
    opts.PublishMessage<VenueCreatedEvent>()
        .ToRabbitExchange("FFXIVVenues.Venue.Events");
    opts.PublishMessage<VenueUpdatedEvent>()
        .ToRabbitExchange("FFXIVVenues.Venue.Events");
    opts.PublishMessage<VenueDeletedEvent>()
        .ToRabbitExchange("FFXIVVenues.Venue.Events");
});

// Configure services

builder.ConfigureAuthorization();
builder.Services.AddSingleton<IMediaRepository>(sp =>
    sp.GetRequiredService<IOptions<MediaConfiguration>>().Value.MediaStorageProvider switch
    {
        "s3" => ActivatorUtilities.CreateInstance<S3MediaRepository>(sp),
        _ => ActivatorUtilities.CreateInstance<LocalMediaRepository>(sp),
    });
builder.Services.Configure<MediaConfiguration>(m => 
{
    m.MediaStorageProvider = config.GetValue<string>("MediaStorage:Provider");
    m.MediaUriTemplate = mediaUriTemplate;
});
builder.Services.AddSecurityServices(o => 
{
    o.SigningPrivateKeyPath = config.GetValue<string>("Security:Signing:Ed25519:PrivateKeyPath") ?? o.SigningPrivateKeyPath;
    o.SigningPublicKeyPath = config.GetValue<string>("Security:Signing:Ed25519:PublicKeyPath") ?? o.SigningPublicKeyPath;
});
builder.Services.AddDomainData(c =>
{
    c.ConnectionString = connectionString;
    c.MediaUriTemplate = mediaUriTemplate;
});
builder.Services.AddSingleton<IEnumerable<AuthorizationKey>>(authorisationKeys);
builder.Services.AddFlagService();
builder.Services.AddSingleton<IAuthorizationProvider, AuthorizationProvider>();
builder.Services.AddSingleton<IAuthorizationManager, AuthorizationManager>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("Discord", c =>
{
    c.BaseAddress = new Uri("https://discord.com/api/v10/");
    c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", discordBotToken);
    c.DefaultRequestHeaders.UserAgent.ParseAdd("DiscordBot (https://ffxivvenues.com, 1.0)");
    c.Timeout = TimeSpan.FromSeconds(10);
}).RedactLoggedHeaders(["Authorization"]);
builder.Services.AddSingleton<IEdmModel>(sp =>
{
    var modelBuilder = new ODataModelBuilder();
    var mediaConfig = sp.GetRequiredService<IOptionsMonitor<MediaConfiguration>>();
    modelBuilder.AddVenuesEdm(mediaConfig);
    modelBuilder.EnableLowerCamelCase();
    return modelBuilder.GetEdmModel();
});
builder.Services.AddControllers().AddOData((o, sp) =>
{
    o.Select().Filter().OrderBy().Expand().Count().SetMaxTop(null);
    var edmModel = sp.GetRequiredService<IEdmModel>();
    o.AddRouteComponents("odata", edmModel, s => s.AddVenuesSerializer());
});
builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new ApiVersion(1, 0);
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.ReportApiVersions = true;
});
builder.Services.AddVersionedApiExplorer(o =>
{
  o.GroupNameFormat = "'v'VV";
  o.DefaultApiVersion = new ApiVersion(1, 0);
  o.AssumeDefaultVersionWhenUnspecified = true;
  o.SubstituteApiVersionInUrl = true;
  o.SubstitutionFormat = "VV";
});
builder.Services.AddVersionedOpenApi(new (1, 0));
builder.Services.AddVersionedOpenApi(new (2, 0));

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

if (builder.Configuration.GetValue("HttpsOnly", true))
    app.UseHttpsRedirection();

app .UseWebSockets()
    .UseRouting()
    .UseCors(
        pb => pb
            .SetIsOriginAllowed(_ => true)
            .AllowAnyMethod()
            .AllowCredentials()
            .AllowAnyHeader()
            .SetPreflightMaxAge(TimeSpan.FromHours(1)));

await app.ConfigureForwardHeaders(config.GetSection("Security:KnownProxies"));
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapOpenApi();
app.UseApiVersioning();
app.MapScalarApiReference(o =>
{
    o.EndpointPathPrefix = "/docs/{documentName}";
    o.Title = "FFXIV Venues API Gateway {documentName}";
});

Log.Information("Starting migrations");
await app.Services.MigrateDomainDataAsync();
Log.Information("Migrations complete");

Log.Information("Starting application");
await app.RunAsync();

public class MediaConfiguration 
{
    public string MediaStorageProvider { get; set; }
    public string MediaUriTemplate { get; set; }
}