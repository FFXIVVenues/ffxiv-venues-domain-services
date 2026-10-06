

using Amazon.Runtime.Internal;
using Castle.Core.Resource;
using FFXIVVenues.ApiGateway.Bootstrap;
using FFXIVVenues.ApiGateway.Controllers.OData;
using FFXIVVenues.ApiGateway.Helpers;
using FFXIVVenues.ApiGateway.Media;
using FFXIVVenues.ApiGateway.Observability;
using FFXIVVenues.ApiGateway.Security;
using FFXIVVenues.DomainData;
using FFXIVVenues.DomainData.Entities.Venues;
using FFXIVVenues.DomainSecurity;
using FFXIVVenues.FlagService.Client;
using FFXIVVenues.FlagService.Client.Events;
using FFXIVVenues.VenueModels;
using FFXIVVenues.VenueModels.Observability;
using FFXIVVenues.VenueService.Client.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Scalar.AspNetCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
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

var rabbitServiceUrl = config.GetValue<string>("Rabbit:ServiceUrl");
var connectionString = config.GetConnectionString("FFXIVVenues");
var mediaUriTemplate = config.GetValue<string>("MediaStorage:UriTemplate");
var mediaStorageProvider = config.GetValue<string>("MediaStorage:Provider");
var authorizationKeys = new List<AuthorizationKey>();
config.GetSection("Security:AuthorizationKeys").Bind(authorizationKeys);

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
    opts.UseRabbitMq(rabbitServiceUrl).AutoProvision();
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
var venueCache = new RollingCache<IEnumerable<FFXIVVenues.VenueModels.Venue>>(3*60*1000, 30*60*1000);

if (mediaStorageProvider.ToLower() == "s3")
    builder.Services.AddSingleton<IMediaRepository, S3MediaRepository>();
else if (mediaStorageProvider.ToLower() == "azure")
    builder.Services.AddSingleton<IMediaRepository, AzureMediaRepository>();
else
    builder.Services.AddSingleton<IMediaRepository, LocalMediaRepository>();

builder.ConfigureAuthorization();
builder.Services.AddDomainData(connectionString, mediaUriTemplate);
builder.Services.AddSecurityServices(o => {
    o.SigningPrivateKeyPath = config.GetValue<string>("Security:Signing:Ed25519:PrivateKeyPath") ?? o.SigningPrivateKeyPath;
    o.SigningPublicKeyPath = config.GetValue<string>("Security:Signing:Ed25519:PublicKeyPath") ?? o.SigningPublicKeyPath;
});
builder.Services.AddSingleton(venueCache);
builder.Services.AddFlagService();
builder.Services.AddSingleton<IAuthorizationManager, AuthorizationManager>();
builder.Services.AddSingleton<IChangeBroker, ChangeBroker>();
builder.Services.AddSingleton<IEnumerable<AuthorizationKey>>(authorizationKeys);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddHttpClient();
builder.Services.AddControllers().AddOData(o => 
{
    
    o.Select().Filter().OrderBy().Expand().Count().SetMaxTop(null);
    var modelBuilder = new ODataModelBuilder();
    modelBuilder.AddVenuesEdm(mediaUriTemplate);
    modelBuilder.EnableLowerCamelCase();
    o.AddRouteComponents("odata", modelBuilder.GetEdmModel(), 
        s => s.AddVenuesSerializer());
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

// Remove this observer and the WebSocket endpoint
// Veni requires fully moving over to subscribing to events
// via Message Queues beforehand
var venueEventsObserver = new Observer([ObservableOperation.Create, ObservableOperation.Update, ObservableOperation.Delete], null, null);
venueEventsObserver.ObserverAction += async (o, e) => {
    using (var serviceScope = app.Services.CreateScope())
    {
        var bus = serviceScope.ServiceProvider.GetService<IMessageBus>();
        object @event = o switch
        {
            ObservableOperation.Create => new VenueCreatedEvent(e.Id),
            ObservableOperation.Update => new VenueUpdatedEvent(e.Id),
            ObservableOperation.Delete => new VenueDeletedEvent(e.Id)
        };
        await bus?.PublishAsync(@event).AsTask();
    }
};
app.Services.GetService<IChangeBroker>()?.Observe(venueEventsObserver, InvocationKind.Delayed);

Log.Information("Starting migrations");
await app.Services.MigrateDomainDataAsync();
Log.Information("Migrations complete");

Log.Information("Starting application");
await app.RunAsync();