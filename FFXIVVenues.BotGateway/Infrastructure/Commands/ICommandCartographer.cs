using System;
using System.Collections.Generic;
using Discord;

namespace FFXIVVenues.Veni.Infrastructure.Commands;

public interface ICommandCartographer
{
    CommandDiscoveryResult Discover();
}

public record struct CommandDiscoveryResult(SlashCommandBuilder[] GlobalCommands,
                                            SlashCommandBuilder[] MasterCommands,
                                            Dictionary<string, Type> Handlers);
