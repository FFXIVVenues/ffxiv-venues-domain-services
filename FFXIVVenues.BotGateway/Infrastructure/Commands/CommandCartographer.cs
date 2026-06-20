using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;

namespace FFXIVVenues.Veni.Infrastructure.Commands;

public class CommandCartographer : ICommandCartographer
{
    public CommandDiscoveryResult Discover()
    {
        var commands = new List<SlashCommandBuilder>();
        var masterCommands = new List<SlashCommandBuilder>();
        var handlers = new Dictionary<string, Type>();
        
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly == null)
            return default;
        
        var types = 
            from type in entryAssembly.GetTypes()
            where Attribute.IsDefined(type, typeof(DiscordCommandAttribute))
            select type;

        foreach (var @type in types)
        {
            var commandAttributes = @type.GetCustomAttributes<DiscordCommandAttribute>()!;
            var optionAttributes = @type.GetCustomAttributes<DiscordCommandOptionAttribute>().ToArray();
            var optionChoiceAttributes = @type.GetCustomAttributes<DiscordCommandOptionChoiceAttribute>().ToArray();
            var isMasterGuildCommand = @type.GetCustomAttributes<DiscordCommandRestrictToMasterGuild>().Any();
            
            foreach (var commandAttribute in commandAttributes)
            {
                
                var commandPath = commandAttribute.Command.Split(' ');
                handlers.Add(string.Join(' ', commandPath), @type);
                var command = GetOrCreateCommand(commands, masterCommands, isMasterGuildCommand, commandPath[0]);
                command.WithContextTypes(command.ContextTypes.Intersect(commandAttribute.ContextTypes).ToArray())
                    .WithDefaultMemberPermissions(command.DefaultMemberPermissions == null
                                                  ? commandAttribute.MemberPermissions
                                                  : command.DefaultMemberPermissions | commandAttribute.MemberPermissions);
                if (commandPath.Length == 1)
                {
                    command.WithDescription(commandAttribute.Description)
                        .WithContextTypes(commandAttribute.ContextTypes)
                        .WithDefaultMemberPermissions(commandAttribute.MemberPermissions);
                    command.AddOptions(AddCommandOptions(optionAttributes, optionChoiceAttributes).ToArray());
                }
                else
                {
                    var subCommand = GetOrCreateSubCommand(command, commandPath[1..]);
                    subCommand.WithDescription(commandAttribute.Description);
                    subCommand.AddOptions(AddCommandOptions(optionAttributes, optionChoiceAttributes).ToArray());
                }
            }
        }

        return new (commands.ToArray(), masterCommands.ToArray(), handlers);
    }

    private static List<SlashCommandOptionBuilder> AddCommandOptions(DiscordCommandOptionAttribute[] optionAttributes,
        DiscordCommandOptionChoiceAttribute[] optionChoiceAttributes)
    {
        var options = new List<SlashCommandOptionBuilder>();
        foreach (var optionAttribute in optionAttributes)
        {
            var option = new SlashCommandOptionBuilder()
                .WithName(optionAttribute.Name)
                .WithDescription(optionAttribute.Description)
                .WithType(optionAttribute.Type)
                .WithRequired(optionAttribute.Required);
            foreach (var optionChoice in optionChoiceAttributes)
            {
                if (optionChoice.OptionName != optionAttribute.Name) continue;
                switch (optionChoice.ChoiceValue)
                {
                    case int value:
                        option.AddChoice(optionChoice.ChoiceName, value);
                        break;
                    case string value:
                        option.AddChoice(optionChoice.ChoiceName, value);
                        break;
                    case double value:
                        option.AddChoice(optionChoice.ChoiceName, value);
                        break;
                    case float value:
                        option.AddChoice(optionChoice.ChoiceName, value);
                        break;
                    case long value:
                        option.AddChoice(optionChoice.ChoiceName, value);
                        break;
                }
            }

            options.Add(option);
        }

        return options;
    }

    private static SlashCommandBuilder GetOrCreateCommand(in List<SlashCommandBuilder> globalCommands,
        in List<SlashCommandBuilder> masterCommands, bool isMaster, string commandName)
    {
        var command = globalCommands.FirstOrDefault(c =>
            c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
        if (isMaster && command is not null)
        {
            // Found in global but needs moving to master
            globalCommands.Remove(command);
            masterCommands.Add(command);
        }
        if (command is null)
        {
            // Not found in global commands, try find in master
            command = masterCommands.FirstOrDefault(c =>
                c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
        }
        if (command is null)
        {
            // Not found anywhere
            command = new SlashCommandBuilder().WithName(commandName).WithDescription("Meow")
                .WithContextTypes(InteractionContextType.PrivateChannel, InteractionContextType.Guild, InteractionContextType.BotDm);
            if (isMaster)
                masterCommands.Add(command);
            else
                globalCommands.Add(command);
        }
        return command;
    }
    
    private static SlashCommandOptionBuilder GetOrCreateSubCommand(in SlashCommandBuilder command, string[] subCommandPath)
    {
        SlashCommandOptionBuilder lastSubCommand = null;
        var options = command.Options ??= new();
        
        foreach (var subCommandKey in subCommandPath)
        {
            var subCommand = options.FirstOrDefault(c =>
                c.Name.Equals(subCommandKey, StringComparison.OrdinalIgnoreCase));
            if (subCommand == null)
            {
                lastSubCommand?.WithType(ApplicationCommandOptionType.SubCommandGroup);
                subCommand = new SlashCommandOptionBuilder().WithName(subCommandKey)
                    .WithDescription("Meow").WithType(ApplicationCommandOptionType.SubCommand);
                options.Add(subCommand);
            }

            options = subCommand.Options ??= new();
            lastSubCommand = subCommand;
        }

        return lastSubCommand;
    }
    
}