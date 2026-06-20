using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;

namespace FFXIVVenues.Veni.VenueAuditing.MassAudit.Commands;

[DiscordCommandRestrictToMasterGuild]
[DiscordCommand("massaudit close", "Closes the last mass audit, disabling all but reporting commands from thereon.")]
public class MassAuditCloseCommand : ICommandHandler
{
    private readonly IAuthorizer _authorizer;
    private readonly IMassAuditService _massAuditService;

    public MassAuditCloseCommand(IAuthorizer authorizer, IMassAuditService massAuditService)
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
        var result = await this._massAuditService.CloseMassAudit();
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
