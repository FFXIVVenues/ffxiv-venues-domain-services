using System.Threading.Tasks;
using Discord;
using FFXIVVenues.BotGateway.Authorisation;
using FFXIVVenues.BotGateway.Authorisation.Blacklist;
using FFXIVVenues.BotGateway.Infrastructure.Commands;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.BotGateway.Utils;
using FFXIVVenues.BotGateway.Infrastructure.Commands.Attributes;

namespace FFXIVVenues.BotGateway.Engineering;

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