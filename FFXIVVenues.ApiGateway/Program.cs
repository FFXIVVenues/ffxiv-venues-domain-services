

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FFXIVVenues.ApiGateway.Helpers;
using FFXIVVenues.ApiGateway.Media;
using FFXIVVenues.ApiGateway.Observability;
using FFXIVVenues.ApiGateway.Security;
using FFXIVVenues.DomainData;
using FFXIVVenues.FlagService.Client;
using FFXIVVenues.VenueModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
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

var betterStackToken = config.GetValue<string>("Logging:BetterStackToken");
var minLevel = config.GetValue<LogEventLevel>("Logging:MinimumLevel");
var rabbitServiceUrl = config.GetValue<string>("Rabbit:ServiceUrl");
var connectionString = config.GetConnectionString("FFXIVVenues");
var mediaUriTemplate = config.GetValue<string>("MediaStorage:UriTemplate");
var mediaStorageProvider = config.GetValue<string>("MediaStorage:Provider");
var authorizationKeys = new List<AuthorizationKey>();
config.GetSection("Security:AuthorizationKeys").Bind(authorizationKeys);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.BetterStack(betterStackToken)
    .MinimumLevel.Is(minLevel)
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
    opts.UseRabbitMq(rabbitServiceUrl).AutoProvision();
    opts.AddFlagServiceMessages();
});

// Configure services
var venueCache = new RollingCache<IEnumerable<Venue>>(3*60*1000, 30*60*1000);

if (mediaStorageProvider.ToLower() == "s3")
    builder.Services.AddSingleton<IMediaRepository, S3MediaRepository>();
else if (mediaStorageProvider.ToLower() == "azure")
    builder.Services.AddSingleton<IMediaRepository, AzureMediaRepository>();
else
    builder.Services.AddSingleton<IMediaRepository, LocalMediaRepository>();

builder.Services.AddOpenApi("v1");
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddApiVersioning();
// builder.Services.AddVersionedApiExplorer();
builder.Services.AddDomainData(connectionString, mediaUriTemplate);
builder.Services.AddSingleton(venueCache);
builder.Services.AddFlagService();
builder.Services.AddSingleton<IAuthorizationManager, AuthorizationManager>();
builder.Services.AddSingleton<IChangeBroker, ChangeBroker>();
builder.Services.AddSingleton<IEnumerable<AuthorizationKey>>(authorizationKeys);
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();



var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

if (builder.Configuration.GetValue("HttpsOnly", true))
    app.UseHttpsRedirection();

app.UseCors(
        pb => pb.SetIsOriginAllowed(_ => true).AllowCredentials().AllowAnyHeader())
    .UseWebSockets()
    .UseRouting();

app.MapControllers();
app.MapOpenApi();
app.MapScalarApiReference(o => 
    o.EndpointPathPrefix = "/docs/{documentName}");

await app.Services.MigrateDomainDataAsync();
await app.RunAsync();