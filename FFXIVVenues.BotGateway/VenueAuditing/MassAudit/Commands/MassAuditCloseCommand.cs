using System.Threading.Tasks;
using FFXIVVenues.BotGateway.Authorisation;
using FFXIVVenues.BotGateway.Infrastructure.Commands;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Commands.Attributes;

namespace FFXIVVenues.BotGateway.VenueAuditing.MassAudit.Commands;

[DiscordCommandRestrictToMasterGuild]
[DiscordCommand("massaudit close", "Closes the last mass audit, disabling all but reporting commands from thereon.")]
public class MassAuditCloseCommand(IAuthorizer authorizer, IMassAuditService massAuditService) : ICommandHandler
{
    public async Task HandleAsync(SlashCommandVeniInteractionContext context)
    {
        var authorized = authorizer.Authorize(context.Interaction.User.Id, Permission.ControlMassAudit);
        if (!authorized.Authorized)
        {
            await context.Interaction.RespondAsync("Sorry, I can't let you do that. 👀", ephemeral: true);
            return;
        }

        await context.Interaction.DeferAsync();
        var result = await massAuditService.CloseMassAudit();
        switch (result)
        {
            case CloseResult.AlreadyClosed:
                await context.Interaction.FollowupAsync("The last mass audit is already closed. 🤔");
                break;
            case CloseResult.StillRunning:
                await context.Interaction.FollowupAsync("The current mass audit is still running. 🤔");
                break;
            case CloseResult.Closed:
                await context.Interaction.FollowupAsync("The mass audit has been closed! 🥳");
                break;
            default:
                await context.Interaction.FollowupAsync("Something wicked this way comes! 🫣");
                break;
        }

    }
}
