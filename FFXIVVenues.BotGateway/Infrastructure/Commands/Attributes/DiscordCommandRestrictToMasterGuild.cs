using System;

namespace FFXIVVenues.BotGateway.Infrastructure.Commands.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]

internal class DiscordCommandRestrictToMasterGuild : Attribute
{
}
