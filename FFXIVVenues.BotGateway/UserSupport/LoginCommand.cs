using Discord;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Infrastructure.Commands;
using FFXIVVenues.BotGateway.Infrastructure.Commands.Attributes;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVVenues.BotGateway.UserSupport;

[DiscordCommand("login", "Login to the website to manage your venues.")]
public class LoginCommand(IApiService apiService) : ICommandHandler
{
    public Task HandleAsync(SlashCommandVeniInteractionContext slashCommand)
    {
        var redirectPath = "/venue/manage";
        var url = apiService.GetSsoUrl(slashCommand.Interaction.User.Id, redirectPath);

        return slashCommand.Interaction.RespondAsync(components: new ComponentBuilder()
            .WithButton("Login to FFXIV Venues", style: ButtonStyle.Link, url: url)
            .Build(), ephemeral: true);
    }
}
