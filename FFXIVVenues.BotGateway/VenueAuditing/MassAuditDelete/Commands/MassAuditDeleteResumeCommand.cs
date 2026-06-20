using System.Threading.Tasks;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.VenueAuditing.MassAudit;

namespace FFXIVVenues.Veni.VenueAuditing.MassAuditDelete.Commands;

[DiscordCommand("massaudit delete resume", "Resume currently paused deletes.")] 
public class MassAuditDeleteResumeCommand(IAuthorizer authorizer, IMassAuditService massAuditService)
    : ICommandHandler
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
        var result = await massAuditService.ResumeDeletesAsync();
        switch (result)
        {
            case ResumeResult.AlreadyRunning:
                await context.Interaction.FollowupAsync("The deletes are already running. 😊");
                break;
            case ResumeResult.NothingToResume:
                await context.Interaction.FollowupAsync("There's no current deletes for this mass audit to resume. 🤔");
                break;
            case ResumeResult.ResumedActive:
                await context.Interaction.FollowupAsync("Deletes that did not gracefully stop has been resumed. 🤔");
                break;
            case ResumeResult.ResumedPaused:
                await context.Interaction.FollowupAsync("The deletes have been resumed. 🥳");
                break;
        }
            
    }
}