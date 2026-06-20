using System.Threading.Tasks;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;

namespace FFXIVVenues.Veni.VenueAuditing.MassAudit.Commands;

[DiscordCommandRestrictToMasterGuild]
[DiscordCommand("massaudit export", "Export a detailed spreadsheet of an audit round.")] 
public class MassAuditExportCommand : ICommandHandler
{
    private readonly IAuthorizer _authorizer;
    private readonly IMassAuditService _massAuditService;

    public MassAuditExportCommand(IAuthorizer authorizer, IMassAuditService massAuditService)
    {
        _authorizer = authorizer;
        _massAuditService = massAuditService;
    }

    public async Task HandleAsync(SlashCommandVeniInteractionContext context)
    {
        var authorized = this._authorizer.Authorize(context.Interaction.User.Id, Permission.ReportMassAudit, null);
        if (!authorized.Authorized)
        {
            await context.Interaction.RespondAsync("Sorry, I can't let you do that. 👀", ephemeral: true);
            return;
        }

        await context.Interaction.DeferAsync();
        
        var report = await this._massAuditService.GetReportAsync();
        if (report == null)
        {
            await context.Interaction.FollowupAsync("There has never been a mass audit to report on.");
            return;
        }

        await context.Interaction.FollowupWithFileAsync(report.ContentStream, report.FileName, "Okay, here it is! 👀");
    }
}
