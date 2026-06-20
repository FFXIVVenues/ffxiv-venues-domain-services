using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Authorisation.Blacklist;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.Engineering;

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