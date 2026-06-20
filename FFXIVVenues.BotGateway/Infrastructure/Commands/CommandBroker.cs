using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;


namespace FFXIVVenues.Veni.Infrastructure.Commands;

internal class CommandBroker(IServiceProvider serviceProvider, IConfiguration config) : ICommandBroker
{
    private readonly DiscordSocketClient _discordClient = serviceProvider.GetService<DiscordSocketClient>();
    private readonly List<SlashCommandProperties> _commands = new();
    private readonly List<SlashCommandProperties> _globalCommands = new();
    private readonly List<SlashCommandProperties> _masterGuildCommands = new();
    private readonly TypeMap<ICommandHandler> _handlers = new(serviceProvider);
    private readonly ICommandCartographer _cartographer = serviceProvider.GetService<ICommandCartographer>();

    public void AddFromAssembly()
    {
        var discoveryResult = this._cartographer.Discover();
        foreach (var handler in discoveryResult.Handlers)
        {
            Log.Debug("Adding slash command {Command} from Assembly", handler.Key);
            this._handlers.Add(handler.Key, handler.Value);
        }
        this._globalCommands.AddRange(discoveryResult.GlobalCommands.Select(c => c.Build()));
        this._masterGuildCommands.AddRange(discoveryResult.MasterCommands.Select(c => c.Build()));
        this._commands.AddRange(_globalCommands);
        this._commands.AddRange(_masterGuildCommands);
    }

    public void Add<TFactory, THandler>(string key, bool isMasterGuildCommand)
        where TFactory : ICommandFactory
        where THandler : ICommandHandler
    {
        Log.Debug("Adding slash command {Command}", key);
        var factory = ActivatorUtilities.CreateInstance<TFactory>(serviceProvider);
        var slashCommand = factory.GetSlashCommand();
        this._commands.Add(slashCommand);
        this._handlers.Add<THandler>(key);
        if (isMasterGuildCommand)
            this._masterGuildCommands.Add(slashCommand);
        else
            this._globalCommands.Add(slashCommand);
    }

    public Task RegisterAllGlobalCommandsAsync()
    {
        Log.Debug("Registering global slash commands");
        return this._discordClient.BulkOverwriteGlobalApplicationCommandsAsync(this._globalCommands.ToArray());
    }

    public Task RegisterMasterGuildCommandsAsync()
    {
        Log.Debug("Fetching master guild");
        var guild = this._discordClient.Guilds.FirstOrDefault(g => g.Id == config.GetValue<ulong>("MasterGuild", 0));
        if (guild == null)
        {
            Log.Warning("Master guild not found");
            return Task.CompletedTask;
        }
        Log.Debug("Registering master guild slash commands with master guild");
        return guild.BulkOverwriteApplicationCommandAsync(this._masterGuildCommands.ToArray());
    }

    public async Task HandleAsync(SlashCommandVeniInteractionContext context)
    {
        var handler = this._handlers.Activate(context.Interaction.CommandName);
        if (handler == null)
        {
            var commandPath = this.GetCommandName(context);
            handler = this._handlers.Activate(commandPath);
        }

        if (handler != null)
            await handler.HandleAsync(context);

        context.TypingHandle?.Dispose();
    }

    private string GetCommandName(SlashCommandVeniInteractionContext context)
    {
        var commandPath = context.Interaction.CommandName;
        var command = this._commands.FirstOrDefault(c => c.Name.Value == commandPath);
        if (command == null) return commandPath;

        var subCommandInteraction = context.Interaction.Data.Options.FirstOrDefault();
        if (subCommandInteraction is null) return commandPath;
        var subCommand = command.Options.Value?.FirstOrDefault(o => o.Name == subCommandInteraction.Name);
        if (subCommand is null) return commandPath;

        do
        {
            if (subCommand.Type is ApplicationCommandOptionType.SubCommand
                or ApplicationCommandOptionType.SubCommandGroup)
                commandPath += " " + subCommandInteraction.Name;

            if (subCommand.Type is not ApplicationCommandOptionType.SubCommandGroup)
                return commandPath;

            subCommandInteraction = subCommandInteraction.Options.FirstOrDefault();
            if (subCommandInteraction is null) return commandPath;
            subCommand = subCommand.Options.FirstOrDefault(o => o.Name == subCommandInteraction.Name);
            if (subCommand is null) return commandPath;
        } while (true);
    }
}