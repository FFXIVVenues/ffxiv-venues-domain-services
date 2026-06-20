using System.Threading.Tasks;
using Discord;
using FFXIVVenues.BotGateway.Authorisation;
using FFXIVVenues.BotGateway.Infrastructure.Commands;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.BotGateway.Utils;
using FFXIVVenues.BotGateway.Authorisation.Blacklist;
using FFXIVVenues.BotGateway.Infrastructure.Commands.Attributes;

namespace FFXIVVenues.BotGateway.Engineering;

[DiscordCommandRestrictToMasterGuild]
[DiscordCommand("blacklist add", "Add a discord guild or user to the blacklist.")]
[DiscordCommandOption("discordid", "Discord ID of guild/user", ApplicationCommandOptionType.String)]
[DiscordCommandOption("reason", "Reason for blacklisting", ApplicationCommandOptionType.String)]
public class BlacklistAddCommand(IRepository db, IAuthorizer authorizer) : ICommandHandler
{
    public async Task HandleAsync(SlashCommandVeniInteractionContext slashCommand)
    {
        if (!authorizer.Authorize(slashCommand.Interaction.User.Id, Permission.Blacklist).Authorized)
            return;

        await slashCommand.Interaction.DeferAsync();
        var discordId = slashCommand.GetStringArg("discordid");
        var reason = slashCommand.GetStringArg("reason");

        var blackListedId = new BlacklistEntry
        {
            id = discordId,
            Reason = reason
        };

        await slashCommand.Interaction.FollowupAsync("Discord ID added to the blacklist 😢");
        await db.UpsertAsync(blackListedId);
    }

}