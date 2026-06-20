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
[DiscordCommand("blacklist remove", "Remove a discord guild or user from the blacklist.")]
[DiscordCommandOption("discordid", "Discord ID of guild/user", ApplicationCommandOptionType.String)]
public class BlacklistRemoveCommand(IRepository db, IAuthorizer authorizer) : ICommandHandler
{
    public async Task HandleAsync(SlashCommandVeniInteractionContext slashCommand)
    {
        if (!authorizer.Authorize(slashCommand.Interaction.User.Id, Permission.Blacklist).Authorized)
            return;

        await slashCommand.Interaction.DeferAsync();
        await db.DeleteAsync<BlacklistEntry>(id: slashCommand.GetStringArg("discordid"));
        await slashCommand.Interaction.FollowupAsync("Discord ID either was removed or wasnt on the blacklist 😊");
    }
}