using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.BotGateway.Authorisation;
using FFXIVVenues.BotGateway.Infrastructure.Commands;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Utils;
using FFXIVVenues.BotGateway.VenueAuditing.MassAudit;
using FFXIVVenues.BotGateway.Infrastructure.Commands.Attributes;

namespace FFXIVVenues.Veni.VenueAuditing.MassAuditNotice.Commands;

[DiscordCommand("massaudit notice start", "Send a notice to all venues from which we're still awaiting response in the current mass audit.")]
[DiscordCommandOption("message", "Custom message to send to all venues.", ApplicationCommandOptionType.String, Required = true)]
public partial class MassAuditNoticeStartCommand(IAuthorizer authorizer, IMassAuditService massAuditService) : ICommandHandler
{
    public async Task HandleAsync(SlashCommandVeniInteractionContext context)
    {
        var authorized = authorizer.Authorize(context.Interaction.User.Id, Permission.ControlMassAudit, null);
        if (!authorized.Authorized)
        {
            await context.Interaction.RespondAsync("Sorry, I can't let you do that. 👀", ephemeral: true);
            return;
        }

        var message = context.GetStringArg("message");
        if (string.IsNullOrWhiteSpace(message))
        {
            await context.Interaction.RespondAsync("No message? 👀", ephemeral: true);
            return;
        }
        
        if (CountPlaceholders(message) != 1)
        {
            await context.Interaction.RespondAsync("There's no placeholder for their list venues in this message. 👀\nAdd a {0} where the manager's list of outstanding venues should be.", ephemeral: true);
            return;
        }

        await context.Interaction.DeferAsync();
        var result = await massAuditService.StartNoticeAsync(context.Interaction.Channel.Id, context.Interaction.User.Id, message);
        switch (result)
        {
            case NoticeResult.Started:
                await context.Interaction.FollowupAsync("The notice is being sent. 😊");
                break;
            case NoticeResult.MassAuditRunning:
            case NoticeResult.MassAuditNotComplete:
                await context.Interaction.FollowupAsync("The current mass audit is still running, we'll need to wait for this to complete before sending a notice.");
                break;
            case NoticeResult.MassAuditClosed:
                await context.Interaction.FollowupAsync("The last mass audit is closed. 🤔");
                break;
            case NoticeResult.NoMassAudits:
                await context.Interaction.FollowupAsync("No mass audit has even been run. 🤔");
                break;
            case NoticeResult.NoticeAlreadyRunning:
                await context.Interaction.FollowupAsync("A notice is already currently running. 🤔");
                break;
            case NoticeResult.NoticePausedExists:
                await context.Interaction.FollowupAsync("A notice has previously started but is paused. 🤔");
                break;
            case NoticeResult.NoticeHaulted:
                await context.Interaction.FollowupAsync("A notice has previously started but is neither complete or currently running. 🤔");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

    }

    private int CountPlaceholders(string input) =>
        GetStringPlaceholdersRegex().Matches(input).Count;
    
    [GeneratedRegex(@"{\d+}")]
    private static partial Regex GetStringPlaceholdersRegex();
    
}