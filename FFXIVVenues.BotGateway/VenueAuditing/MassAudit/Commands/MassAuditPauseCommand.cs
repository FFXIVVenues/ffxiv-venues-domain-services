using System.Threading.Tasks;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;

namespace FFXIVVenues.Veni.VenueAuditing.MassAudit.Commands;

[DiscordCommandRestrictToMasterGuild]
[DiscordCommand("massaudit pause", "Pause a currently executing audit round, it may be resumed after.")] 
public class MassAuditPauseCommand : ICommandHandler
{
    private readonly IAuthorizer _authorizer;
    private readonly IMassAuditService _massAuditService;

    public MassAuditPauseCommand(IAuthorizer authorizer, IMassAuditService massAuditService)
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
        var result = await this._massAuditService.PauseAsync();
        switch (result)
        {
            case PauseResult.NothingToPause:
                await context.Interaction.FollowupAsync("There's no current mass audit to pause. 🤔");
                break;
            case PauseResult.Closed:
                await context.Interaction.FollowupAsync("Last mass audit is closed. 🤔");
                break;
            case PauseResult.AlreadyPaused:
                await context.Interaction.FollowupAsync("Current mass audit is already paused. 🤔");
                break;
            case PauseResult.Paused:
                await context.Interaction.FollowupAsync("I've paused the mass audit! 👀");
                break;
        }
    }
}
