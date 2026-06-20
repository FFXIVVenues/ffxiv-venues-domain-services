using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace FFXIVVenues.Veni;

internal static partial class Bootstrap
{

    internal static DiscordSocketClient ConfigureDiscordClient(IServiceCollection serviceCollection, Configurations config)
    {
        if (config.DiscordToken == null)
            throw new Exception("Discord Bot Token not set!");

        var client = new DiscordSocketClient(new()
        {
            LogLevel = LogSeverity.Verbose,
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
        });
        client.Log += (LogMessage msg) =>
        {
            var level = msg.Severity switch
            {
                LogSeverity.Critical => LogEventLevel.Fatal,
                LogSeverity.Error => LogEventLevel.Error,
                LogSeverity.Warning => LogEventLevel.Warning,
                LogSeverity.Info => LogEventLevel.Information,
                LogSeverity.Debug => LogEventLevel.Debug,
                LogSeverity.Verbose => LogEventLevel.Verbose,
                _ => LogEventLevel.Debug
            };
            Log.Write(level, msg.Message, msg.Exception);
            return Task.CompletedTask;
        };
        client.LoginAsync(TokenType.Bot, config.DiscordToken);
        
        serviceCollection.AddSingleton<IDiscordClient>(client);
        serviceCollection.AddSingleton(client);
        
        return client;
    }
}