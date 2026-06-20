using System.Threading.Tasks;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;

namespace FFXIVVenues.Veni.VenueAuditing.MassAudit.Commands;

[DiscordCommandRestrictToMasterGuild]
[DiscordCommand("massaudit cancel", "Cancel a currently executing audit round.")] 
public class MassAuditCancelCommand : ICommandHandler
{
    private readonly IAuthorizer _authorizer;
    private readonly IMassAuditService _massAuditService;

    public MassAuditCancelCommand(IAuthorizer authorizer, IMassAuditService massAuditService)
    {
        _authorizer = authorizer;
        _massAuditService = massAuditService;
    }

    public async Task HandleAsync(SlashCommandVeniInteractionContext context)
    {
        var authorized = this._authorizer.Authorize(context.Interaction.User.Id, Permission.ControlMassAudit, null);
        if (!authorized.Authorized)
        {
            await context.Interaction.RespondAsync("Sorry, I can't let you do that. 👀", ephemeral: true);
            return;
        }

        await context.Interaction.DeferAsync();
        var result = await this._massAuditService.CancelAsync();
        switch (result)
        {
            case CancelResult.NothingToCancel:
                await context.Interaction.FollowupAsync("There's no active mass audit to cancel. 🤔");
                break;
            case CancelResult.Cancelled:
                await context.Interaction.FollowupAsync("Cancelled! 👀");
                break;
        }
    }
}
