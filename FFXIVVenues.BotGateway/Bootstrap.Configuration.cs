using FFXIVVenues.BotGateway.AI.Clu;
using FFXIVVenues.BotGateway.AI.Davinci;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Authorisation.Configuration;
using FFXIVVenues.BotGateway.Infrastructure;
using FFXIVVenues.BotGateway.Infrastructure.Persistence;
using FFXIVVenues.BotGateway.Infrastructure.Presence;
using FFXIVVenues.BotGateway.Infrastructure.Security;
using FFXIVVenues.BotGateway.VenueControl.VenueAuthoring;
using FFXIVVenues.BotGateway.VenueEvents;
using FFXIVVenues.BotGateway.VenueRendering;
using FFXIVVenues.DomainSecurity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace FFXIVVenues.Veni;

internal static partial class Bootstrap
{
    internal static Configurations LoadConfiguration(IServiceCollection serviceCollection)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("config.json", optional: true)
            .AddUserSecrets<DiscordHostedService>(optional: true)
            .AddEnvironmentVariables("FFXIV_VENUES_")
            .AddEnvironmentVariables("FFXIV_VENUES_VENI_")
            .Build();

        var allConfig = new Configurations
        {
            ConnectionString = config.GetConnectionString("FFXIVVenues"),

            DiscordToken = config.GetValue<string>("DiscordBotToken") ?? throw new Exception("DiscordBotToken configuration not set"),
            LoggingConfig = config.GetSection("Logging").Get<LoggingConfiguration>() ?? new(),
            LuisConfig = config.GetSection("Clu").Get<CluConfiguration>() ?? new(),
            ApiConfig = config.GetSection("Api").Get<ApiConfiguration>() ?? new(),
            PersistenceConfig = config.GetSection("Persistence").Get<PersistenceConfiguration>() ?? new(),
            UiConfig = config.GetSection("Ui").Get<UiConfiguration>() ?? new(),
            NotificationConfig = config.GetSection("Notifications").Get<NotificationsConfiguration>() ?? new(),
            AuthorisationConfig = config.GetSection("Authorisation").Get<AuthorisationConfiguration>() ?? new(),
            DavinciConfig = config.GetSection("Davinci3").Get<DavinciConfiguration>() ?? new(),
            PresenceConfig = config.GetSection("Presence").Get<PresenceConfiguration>() ?? new(),
            RabbitConfig = config.GetSection("Rabbit").Get<RabbitConfiguration>() ?? new(),
            SecurityConfig = config.GetSection("Security").Get<SecurityConfiguration>() ?? new(),
        };

        if (allConfig.ConnectionString is null) throw new Exception("Configuration ConnectionStrings:FFXIVVenues not set");
        if (allConfig.DiscordToken is null) throw new Exception("Configuration DiscordBotToken not set");
        if (allConfig.AuthorisationConfig.ManagerPermissions.Length == 0) throw new Exception("Configuration Authorisation:ManagerPermissions not set");
        if (allConfig.AuthorisationConfig.Master.Length == 0) throw new Exception("Configuration Authorisation:Master not set");
        if (allConfig.ApiConfig.BaseUrl is null) throw new Exception("Configuration Api:BaseUrl not set");
        if (allConfig.ApiConfig.AuthorizationKey is null) throw new Exception("Configuration Api:AuthorizationKey not set");
        if (allConfig.UiConfig.MediaUriTemplate is null) throw new Exception("Configuration Ui:MediaUriTemplate not set");
        if (allConfig.UiConfig.BaseUrl is null) throw new Exception("Configuration Ui:BaseUrl not set");
        if (allConfig.RabbitConfig.ServiceUrl is null) throw new Exception("Configuration Rabbit:ServiceUrl not set");

        serviceCollection.AddSingleton<IConfiguration>(config);
        serviceCollection.AddSingleton(allConfig.LuisConfig);
        serviceCollection.AddSingleton(allConfig.AuthorisationConfig);
        serviceCollection.AddSingleton(allConfig.NotificationConfig);
        serviceCollection.AddSingleton(allConfig.DavinciConfig);
        serviceCollection.AddSingleton(allConfig.ApiConfig);
        serviceCollection.AddSingleton(allConfig.PersistenceConfig);
        serviceCollection.AddSingleton(allConfig.UiConfig);
        serviceCollection.AddSingleton(allConfig.PresenceConfig);
        serviceCollection.AddSingleton(allConfig.RabbitConfig);

        return allConfig;
    }
}

internal class Configurations
{
    public string DiscordToken { get; set; }
    public LoggingConfiguration LoggingConfig { get; set; }
    public CluConfiguration LuisConfig { get; set; }
    public ApiConfiguration ApiConfig { get; set; }
    public PersistenceConfiguration PersistenceConfig { get; set; }
    public UiConfiguration UiConfig { get; set; }
    public NotificationsConfiguration NotificationConfig { get; set; }
    public AuthorisationConfiguration AuthorisationConfig { get; set; }
    public DavinciConfiguration DavinciConfig { get; set; }
    public PresenceConfiguration PresenceConfig { get; set; }
    public RabbitConfiguration RabbitConfig { get; set; }
    public SecurityConfiguration SecurityConfig { get; set; }
    public string ConnectionString { get; set; }
}
