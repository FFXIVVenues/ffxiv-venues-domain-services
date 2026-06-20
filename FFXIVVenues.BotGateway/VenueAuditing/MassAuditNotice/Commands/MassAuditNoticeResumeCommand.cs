using System.Threading.Tasks;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.VenueAuditing.MassAudit;

namespace FFXIVVenues.Veni.VenueAuditing.MassAuditNotice.Commands
{
    [DiscordCommand("massaudit notice resume", "Resume a currently paused notice.")] 
    public class MassAuditNoticeResumeCommand(IAuthorizer authorizer, IMassAuditService massAuditService)
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
            var result = await massAuditService.ResumeNoticeAsync();
            switch (result)
            {
                case ResumeResult.AlreadyRunning:
                    await context.Interaction.FollowupAsync("The notice is already running. 😊");
                    break;
                case ResumeResult.NothingToResume:
                    await context.Interaction.FollowupAsync("There's no current notice for this mass audit to resume. 🤔");
                    break;
                case ResumeResult.ResumedActive:
                    await context.Interaction.FollowupAsync("A notice that did not gracefully stop has been resumed. 🤔");
                    break;
                case ResumeResult.ResumedPaused:
                    await context.Interaction.FollowupAsync("The notice has been resumed. 🥳");
                    break;
            }
            
        }
    }
}
