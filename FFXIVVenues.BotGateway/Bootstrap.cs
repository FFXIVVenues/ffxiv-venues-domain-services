using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using FFXIVVenues.Veni;
using Microsoft.Extensions.Hosting;
using OfficeOpenXml;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.AI.Clu;
using FFXIVVenues.BotGateway.VenueControl.VenueAuthoring;
using FFXIVVenues.BotGateway.VenueObservations;
using FFXIVVenues.BotGateway.Authorisation;
using FFXIVVenues.BotGateway.Infrastructure.Commands;
using FFXIVVenues.BotGateway.VenueAuditing.MassAudit;
using FFXIVVenues.BotGateway.VenueControl;
using FFXIVVenues.BotGateway.VenueRendering;
using FFXIVVenues.BotGateway.Infrastructure;
using FFXIVVenues.BotGateway.VenueAuditing.MassAudit.Exporting;
using FFXIVVenues.BotGateway.VenueAuditing.MassAuditNotice;
using FFXIVVenues.BotGateway.VenueAuditing.MassAuditDelete;
using FFXIVVenues.BotGateway.VenueAuditing;
using FFXIVVenues.BotGateway.Infrastructure.Components;
using FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.VenueApproval;
using FFXIVVenues.BotGateway.AI.Davinci;
using FFXIVVenues.BotGateway.Infrastructure.Context.Abstractions;
using FFXIVVenues.BotGateway.Infrastructure.Context.SessionHandling;
using FFXIVVenues.BotGateway.GuildEngagement;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Infrastructure.Intent;
using FFXIVVenues.BotGateway.Infrastructure.Presence;
using FFXIVVenues.BotGateway.UserSupport;
using FFXIVVenues.BotGateway.VenueDiscovery.Commands;

var builder = Host.CreateApplicationBuilder(args);

var config = Bootstrap.LoadConfiguration(builder.Services);
Bootstrap.ConfigureLogging(builder, config);
Bootstrap.ConfigureApiClient(builder.Services, config);
Bootstrap.ConfigureRepository(builder.Services, config);
Bootstrap.ConfigureDiscordClient(builder.Services, config);
Bootstrap.ConfigureRabbit(builder, config);

ExcelPackage.License.SetNonCommercialOrganization("FFXIV Venues");

builder.Services.AddSingleton<ICommandBroker, CommandBroker>();
builder.Services.AddSingleton<IComponentBroker, ComponentBroker>();
builder.Services.AddSingleton<IApiService, ApiService>();
builder.Services.AddSingleton<IAuthorizer, Authorizer>();
builder.Services.AddSingleton<IGuildManager, GuildManager>();
builder.Services.AddSingleton<IVenueApprovalService, VenueApprovalService>();
builder.Services.AddSingleton<IAIHandler, AIHandler>();
builder.Services.AddSingleton<IDavinciService, DavinciService>();
builder.Services.AddSingleton<IAIContextBuilder, AiContextBuilder>();
builder.Services.AddSingleton<IIntentHandlerProvider, IntentHandlerProvider>();
builder.Services.AddSingleton<ISessionProvider, SessionProvider>();
builder.Services.AddSingleton<ICluClient, CluClient>();
builder.Services.AddSingleton<IVenueAuditService, VenueAuditService>();
builder.Services.AddSingleton<IVenueRenderer, VenueRenderer>();
builder.Services.AddSingleton<IApiObservationService, ApiObservationService>();
builder.Services.AddSingleton<IInteractionContextFactory, InteractionContextFactory>();
builder.Services.AddSingleton<ICommandCartographer, CommandCartographer>();
builder.Services.AddSingleton<IMassAuditService, MassAuditService>();
builder.Services.AddSingleton<IMassAuditExporter, MassAuditExporter>();
builder.Services.AddSingleton<MassNoticeService>();
builder.Services.AddSingleton<MassDeleteService>();
builder.Services.AddSingleton<IDiscordValidator, DiscordValidator>();
builder.Services.AddSingleton<ISiteValidator, SiteValidator>();
builder.Services.AddSingleton<IActivityManager, ActivityManager>();

builder.Services.AddHostedService<DiscordHostedService>();

var app = builder.Build();

var commandBroker = app.Services.GetService<ICommandBroker>();
commandBroker.AddFromAssembly();
commandBroker.AddVenueControlCommands();
commandBroker.Add<HelpCommand.CommandFactory, HelpCommand.CommandHandler>(HelpCommand.COMMAND_NAME, isMasterGuildCommand: false);
commandBroker.Add<ShowOpenCommand.CommandFactory, ShowOpenCommand.CommandHandler>(ShowOpenCommand.COMMAND_NAME, isMasterGuildCommand: false);
commandBroker.Add<ShowForCommand.CommandFactory, ShowForCommand.CommandHandler>(ShowForCommand.COMMAND_NAME, isMasterGuildCommand: false);
commandBroker.Add<ShowMineCommand.CommandFactory, ShowMineCommand.CommandHandler>(ShowMineCommand.COMMAND_NAME, isMasterGuildCommand: false);
commandBroker.Add<ShowCountCommand.CommandFactory, ShowCountCommand.CommandHandler>(ShowCountCommand.COMMAND_NAME, isMasterGuildCommand: false);
commandBroker.Add<GetUnapprovedCommand.CommandFactory, GetUnapprovedCommand.CommandHandler>(GetUnapprovedCommand.COMMAND_NAME, isMasterGuildCommand: false);

app.Services.GetService<IComponentBroker>()
    .AddVenueObservationHandlers()
    .AddVenueAuditingHandlers()
    .AddVenueControlHandlers()
    .AddVenueRenderingHandlers();

_ = app.Services.GetService<IApiObservationService>()
    .AddVenueObservers()
    .ObserveAsync();

await app.RunAsync();
