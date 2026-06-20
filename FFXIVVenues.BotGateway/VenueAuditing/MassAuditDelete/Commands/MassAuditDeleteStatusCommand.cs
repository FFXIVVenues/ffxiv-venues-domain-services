using System.Text;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.VenueAuditing.MassAudit;

namespace FFXIVVenues.Veni.VenueAuditing.MassAuditDelete.Commands;

[DiscordCommand("massaudit delete status", "Get a brief update on the progress of deletes.")] 
public class MassAuditDeleteStatusCommand(IAuthorizer authorizer, IMassAuditService massAuditService) : ICommandHandler
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

        var summary = await massAuditService.GetDeletesSummaryAsync();
        if (summary == null)
        {
            await context.Interaction.FollowupAsync("There has never been deletes for this mass audit.");
            return;
        }

        var builder = new StringBuilder()
            .Append("**Id**: ").Append(summary.id).AppendLine()
            .Append("**Mass Audit Id**: ").Append(summary.MassAuditId).AppendLine()
            .Append("**Status**: ").Append(summary.Status).AppendLine()
            .Append("**Requested By**: ").Append(MentionUtils.MentionUser(summary.RequestedBy)).AppendLine()
            .Append("**Started At**: ").Append(summary.StartedAt?.ToString("g")).AppendLine()
            .Append("**Paused At**: ").Append(summary.PausedAt?.ToString("g")).AppendLine()
            .Append("**Completed At**: ").Append(summary.CompletedAt?.ToString("g")).AppendLine()
            .AppendLine()
            .Append("**Total Deletes**: ").Append(summary.TotalVenues).AppendLine()
            .Append("**Deletes Complete**: ").Append(summary.VenuesDeleted).AppendLine()
            .Append("**Deletes Failed**: ").Append(summary.VenuesFailedToDelete).AppendLine()
            .Append("**Deleted Pending**: ").Append(summary.VenuesPending).AppendLine();
        var embedBuilder = new EmbedBuilder()
            .WithTitle("Deletes executed on " + summary.StartedAt?.ToString("dd MMMM yyyy"))
            .WithDescription(builder.ToString());
            
        await context.Interaction.FollowupAsync("Okay, here it is! 🥰", embed: embedBuilder.Build());
    }
}