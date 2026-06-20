using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueAuditing.MassAudit;

namespace FFXIVVenues.Veni.VenueAuditing.MassAuditDelete.Commands;

[DiscordCommand("massaudit delete start", "Delete all venues that have no responded to the current mass audit.")]
public class MassAuditDeleteStartCommand(IAuthorizer authorizer, IMassAuditService massAuditService) : ICommandHandler
{
    public async Task HandleAsync(SlashCommandVeniInteractionContext context)
    {
        var authorizedToControlMassAudit = authorizer.Authorize(context.Interaction.User.Id, Permission.ControlMassAudit, null);
        var authorizedToDelete = authorizer.Authorize(context.Interaction.User.Id, Permission.DeleteVenue, null);
        var authorized = authorizedToControlMassAudit.Authorized && authorizedToDelete.Authorized;
        if (!authorized)
        {
            await context.Interaction.RespondAsync("Sorry, I can't let you do that. 👀", ephemeral: true);
            return;
        }

        await context.Interaction.DeferAsync();
        var result =
            await massAuditService.StartDeletesAsync(context.Interaction.Channel.Id, context.Interaction.User.Id);
        switch (result)
        {
            case DeleteResult.Started:
                await context.Interaction.FollowupAsync("Deletes have started 😊");
                break;
            case DeleteResult.MassAuditRunning:
            case DeleteResult.MassAuditNotComplete:
                await context.Interaction.FollowupAsync(
                    "The current mass audit is still running, now is not the time to delete them.");
                break;
            case DeleteResult.MassAuditClosed:
                await context.Interaction.FollowupAsync("The last mass audit is closed. 🤔");
                break;
            case DeleteResult.NoMassAudits:
                await context.Interaction.FollowupAsync("No mass audit has even been run. 🤔");
                break;
            case DeleteResult.DeleteAlreadyRunning:
                await context.Interaction.FollowupAsync("Deletes are already running. 🤔");
                break;
            case DeleteResult.DeletePausedExists:
                await context.Interaction.FollowupAsync("Deletes have previously started but is paused. 🤔");
                break;
            case DeleteResult.DeleteHaulted:
                await context.Interaction.FollowupAsync(
                    "Delete have previously started but is neither complete or currently running. 🤔");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

    }
}