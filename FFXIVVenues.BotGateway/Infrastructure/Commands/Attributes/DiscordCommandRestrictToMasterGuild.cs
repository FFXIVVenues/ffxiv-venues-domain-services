using System;

namespace FFXIVVenues.Veni.Infrastructure.Commands.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]

internal class DiscordCommandRestrictToMasterGuild : Attribute
{
}
