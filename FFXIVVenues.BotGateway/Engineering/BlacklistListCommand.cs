using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Authorisation.Blacklist;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;

namespace FFXIVVenues.Veni.Engineering;

[DiscordCommandRestrictToMasterGuild]
[DiscordCommand("blacklist list", "List of blacklisted users/servers.")]
public class BlacklistListCommand : ICommandHandler
{
    private readonly IRepository _db;
    private readonly IAuthorizer _authorizer;

    public BlacklistListCommand(IRepository db, IAuthorizer authorizer)
    {
        this._db = db;
        this._authorizer = authorizer;
    }

    public async Task HandleAsync(SlashCommandVeniInteractionContext slashCommand)
    {
        if (!_authorizer.Authorize(slashCommand.Interaction.User.Id, Permission.Blacklist).Authorized)
            return;

        await slashCommand.Interaction.DeferAsync();
        var bannedIdList = await _db.QueryAsync<BlacklistEntry>().ContinueWith(t => t.Result.ToList());
        var description = new StringBuilder();

        if (bannedIdList.Any() == false)
        {
            description.Append("There are no blacklisted IDs ☺️");
        }
        foreach (var banned in bannedIdList){
            description.Append("**");
            description.Append(banned.id);
            description.Append("**: ");
            description.Append(banned.Reason);
            description.AppendLine();
        }
        var embed = new EmbedBuilder()
            .WithTitle("Blacklist")
            .WithDescription(description.ToString())
            .Build();

        await slashCommand.Interaction.FollowupAsync(embed:embed);
    }
}