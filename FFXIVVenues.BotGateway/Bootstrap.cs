using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using FFXIVVenues.Veni;
using FFXIVVenues.Veni.AI.Clu;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.Abstractions;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.VenueAuditing;
using FFXIVVenues.Veni.VenueControl;
using FFXIVVenues.Veni.AI.Davinci;
using FFXIVVenues.Veni.AI.Luis;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.GuildEngagement;
using FFXIVVenues.Veni.Infrastructure;
using FFXIVVenues.Veni.Infrastructure.Presence;
using FFXIVVenues.Veni.UserSupport;
using FFXIVVenues.Veni.VenueAuditing.MassAudit;
using FFXIVVenues.Veni.VenueAuditing.MassAudit.Exporting;
using FFXIVVenues.Veni.VenueAuditing.MassAuditDelete;
using FFXIVVenues.Veni.VenueAuditing.MassAuditNotice;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueApproval;
using FFXIVVenues.Veni.VenueDiscovery.Commands;
using FFXIVVenues.Veni.VenueObservations;
using FFXIVVenues.Veni.VenueRendering;
using Microsoft.Extensions.Hosting;
using OfficeOpenXml;

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
