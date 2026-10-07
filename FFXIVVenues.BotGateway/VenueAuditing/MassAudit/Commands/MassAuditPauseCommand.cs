using System.Threading.Tasks;
using FFXIVVenues.BotGateway.Authorisation;
using FFXIVVenues.BotGateway.Infrastructure.Commands;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.VenueAuditing.MassAudit;
using FFXIVVenues.BotGateway.Infrastructure.Commands.Attributes;

namespace FFXIVVenues.BotGateway.VenueAuditing.MassAudit.Commands;

[DiscordCommandRestrictToMasterGuild]
[DiscordCommand("massaudit pause", "Pause a currently executing audit round, it may be resumed after.")] 
public class MassAuditPauseCommand(IAuthorizer authorizer, IMassAuditService massAuditService) : ICommandHandler
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
        var result = await massAuditService.PauseAsync();
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
