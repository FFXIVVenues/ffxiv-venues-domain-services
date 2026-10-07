using System.Threading.Tasks;
using FFXIVVenues.BotGateway.Authorisation;
using FFXIVVenues.BotGateway.Infrastructure.Commands;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Commands.Attributes;

namespace FFXIVVenues.BotGateway.VenueAuditing.MassAudit.Commands;

[DiscordCommandRestrictToMasterGuild]
[DiscordCommand("massaudit export", "Export a detailed spreadsheet of an audit round.")] 
public class MassAuditExportCommand(IAuthorizer authorizer, IMassAuditService massAuditService) : ICommandHandler
{
    public async Task HandleAsync(SlashCommandVeniInteractionContext context)
    {
        var authorized = authorizer.Authorize(context.Interaction.User.Id, Permission.ReportMassAudit);
        if (!authorized.Authorized)
        {
            await context.Interaction.RespondAsync("Sorry, I can't let you do that. 👀", ephemeral: true);
            return;
        }

        await context.Interaction.DeferAsync();
        
        var report = await massAuditService.GetReportAsync();
        if (report == null)
        {
            await context.Interaction.FollowupAsync("There has never been a mass audit to report on.");
            return;
        }

        await context.Interaction.FollowupWithFileAsync(report.ContentStream, report.FileName, "Okay, here it is! 👀");
    }
}
