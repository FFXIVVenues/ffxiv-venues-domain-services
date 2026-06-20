using System;
using System.Linq;
using Discord;

namespace FFXIVVenues.Veni.Infrastructure.Commands.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class DiscordCommandAttribute(
    string command,
    string description,
    GuildPermission memberPermissions = 0,
    params InteractionContextType[] contextTypes)
    : Attribute
{
    public string Command { get; } = command;
    public string Description { get; } = description;
    public GuildPermission? MemberPermissions { get; } = memberPermissions == 0 ? null : memberPermissions;
    public InteractionContextType[] ContextTypes { get; } = contextTypes.Any() ? contextTypes : 
        [ InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel ];
}