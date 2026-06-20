using System.Text;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.VenueAuditing.MassAudit;

namespace FFXIVVenues.Veni.VenueAuditing.MassAuditNotice.Commands
{
    [DiscordCommand("massaudit notice status", "Get a brief update on the progress of a current notice.")] 
    public class MassAuditNoticeStatusCommand(IAuthorizer authorizer, IMassAuditService massAuditService) : ICommandHandler
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

            var summary = await massAuditService.GetNoticeSummaryAsync();
            if (summary == null)
            {
                await context.Interaction.FollowupAsync("There has never been a notice for this mass audit.");
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
                .Append("**Total users**: ").Append(summary.TotalUsers).AppendLine()
                .Append("**Notices Sent**: ").Append(summary.NoticesSent).AppendLine()
                .Append("**Notices Failed**: ").Append(summary.NoticesFailed).AppendLine()
                .Append("**Notices Pending**: ").Append(summary.NoticesPending).AppendLine();
            var embedBuilder = new EmbedBuilder()
                .WithTitle("Notice sent on " + summary.StartedAt?.ToString("dd MMMM yyyy"))
                .WithDescription(builder.ToString());
            
            await context.Interaction.FollowupAsync("Okay, here it is! 🥰", embed: embedBuilder.Build());
        }
    }
}
