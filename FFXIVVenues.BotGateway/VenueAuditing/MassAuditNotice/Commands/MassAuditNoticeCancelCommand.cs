using System.Threading.Tasks;
using FFXIVVenues.BotGateway.Authorisation;
using FFXIVVenues.BotGateway.Infrastructure.Commands;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.VenueAuditing.MassAudit;
using FFXIVVenues.BotGateway.Infrastructure.Commands.Attributes;

namespace FFXIVVenues.BotGateway.VenueAuditing.MassAuditNotice.Commands;

[DiscordCommand("massaudit notice cancel", "Cancel a currently executing notice.")]
public class MassAuditNoticeCancelCommand(IAuthorizer authorizer, IMassAuditService massAuditService) : ICommandHandler
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
        var result = await massAuditService.CancelNoticeAsync();
        switch (result)
        {
            case CancelResult.NothingToCancel:
                await context.Interaction.FollowupAsync("There's no running notice for this mass audit to cancel. 🤔");
                break;
            case CancelResult.Cancelled:
                await context.Interaction.FollowupAsync("Cancelled! 👀");
                break;
        }
    }
    
}