using System.Threading.Tasks;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.VenueAuditing.MassAudit;

namespace FFXIVVenues.Veni.VenueAuditing.MassAuditDelete.Commands;

[DiscordCommand("massaudit delete pause", "Pause currently executing deletes, they may be resumed after.")]
public class MassAuditDeletePauseCommand(IAuthorizer authorizer, IMassAuditService massAuditService) : ICommandHandler
{
    public async Task HandleAsync(SlashCommandVeniInteractionContext context)
    {
        var authorized = authorizer.Authorize(context.Interaction.User.Id, Permission.ControlMassAudit, null);
        if (!authorized.Authorized)
        {
            await context.Interaction.RespondAsync("Sorry, I can't let you do that. 👀", ephemeral: true);
            return;
        }

        await context.Interaction.DeferAsync();
        var result = await massAuditService.PauseDeletesAsync();
        switch (result)
        {
            case PauseResult.NothingToPause:
                await context.Interaction.FollowupAsync("There's no current deletes for this mass audit to pause. 🤔");
                break;
            case PauseResult.Closed:
                await context.Interaction.FollowupAsync("The most recent deletes are closed. 🤔");
                break;
            case PauseResult.AlreadyPaused:
                await context.Interaction.FollowupAsync("The current deletes for this mass audit is already paused. 🤔");
                break;
            case PauseResult.Paused:
                await context.Interaction.FollowupAsync("I've paused the deletes! 👀");
                break;
        }
    }
    
}