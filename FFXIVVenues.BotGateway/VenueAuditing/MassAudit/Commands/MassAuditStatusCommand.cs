using System.Text;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;

namespace FFXIVVenues.Veni.VenueAuditing.MassAudit.Commands;

[DiscordCommandRestrictToMasterGuild]
[DiscordCommand("massaudit status", "Get a brief update on the progress of a current audit round.")] 
public class MassAuditStatusCommand(IAuthorizer authorizer, IMassAuditService massAuditService) : ICommandHandler
{
    public async Task HandleAsync(SlashCommandVeniInteractionContext context)
    {
        var authorized = authorizer.Authorize(context.Interaction.User.Id, Permission.ReportMassAudit, null);
        if (!authorized.Authorized)
        {
            await context.Interaction.RespondAsync("Sorry, I can't let you do that. 👀", ephemeral: true);
            return;
        }
        
        await context.Interaction.DeferAsync();
        context.TypingHandle?.Dispose();

        var summary = await massAuditService.GetSummaryAsync();
        if (summary == null)
        {
            await context.Interaction.FollowupAsync("There has never been a mass audit to summarize.");
            return;
        }

        var builder = new StringBuilder()
            .Append("**Id**: ").Append(summary.id).AppendLine()
            .Append("**Status**: ").Append(summary.Status).AppendLine()
            .Append("**Requested By**: ").Append(MentionUtils.MentionUser(summary.RequestedBy)).AppendLine()
            .Append("**Started At**: ").Append(summary.StartedAt?.ToString("g")).AppendLine()
            .Append("**Paused At**: ").Append(summary.PausedAt?.ToString("g")).AppendLine()
            .Append("**Completed At**: ").Append(summary.CompletedAt?.ToString("g")).AppendLine()
            .AppendLine()
            .Append("**Total venues to audit**: ").Append(summary.TotalVenues).AppendLine()
            .Append("**Audits processed so far**: ").Append(summary.AuditsProcessed).AppendLine()
            .Append("**Audits answered so far**: ").Append(summary.AuditsAnswered).AppendLine()
            .AppendLine()
            .Append("**Venues confirmed**: ").Append(summary.VenuesConfirmed).AppendLine()
            .Append("**Venues edited**: ").Append(summary.VenuesEdited).AppendLine()
            .Append("**Venues closed**: ").Append(summary.VenuesClosed).AppendLine()
            .Append("**Venues deleted**: ").Append(summary.VenuesDeleted).AppendLine()
            .AppendLine()
            .Append("**Audits awaiting answer**: ").Append(summary.AuditsAwaitingAnswer).AppendLine()
            .Append("**Audits skipped**: ").Append(summary.AuditsSkipped).AppendLine()
            .Append("**Audits failed**: ").Append(summary.AuditsFailed).AppendLine()
            .Append("**Audits in progress**: ").Append(summary.AuditsInProgress);
        var embedBuilder = new EmbedBuilder()
            .WithTitle("Audit started on " + summary.StartedAt?.ToString("dd MMMM yyyy"))
            .WithDescription(builder.ToString());
        
        await context.Interaction.FollowupAsync("Okay, here it is! 🥰", embed: embedBuilder.Build());
    }
}
