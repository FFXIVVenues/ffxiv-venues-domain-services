using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.VenueAuditing.ComponentHandlers.AuditResponse;
using FFXIVVenues.Veni.VenueRendering.ComponentHandlers;

namespace FFXIVVenues.Veni.VenueAuditing.ComponentHandlers;

public class GetAuditHandler : IComponentHandler
{

    // Change this key and any existing buttons linked to this will die
    public static string Key => "CONTROL_GET_AUDIT";

    private readonly IAuthorizer _authorizer;
    private readonly IApiService _apiService;
    private readonly IRepository _repository;

    public GetAuditHandler(IAuthorizer authorizer, IApiService apiService, IRepository repository)
    {
        this._authorizer = authorizer;
        this._apiService = apiService;
        this._repository = repository;
    }
    
    public async Task HandleAsync(ComponentVeniInteractionContext context, string[] args)
    {
        var user = context.Interaction.User.Id;
        var auditId = context.Interaction.Data.Values.First();
        var audit = await this._repository.GetByIdAsync<VenueAuditRecord>(auditId);
        var venue = await this._apiService.GetVenueAsync(audit.VenueId);
        
        if (!this._authorizer.Authorize(user, Permission.ViewAuditHistory, venue).Authorized)
            return;
        
        _ = context.Interaction.DeleteOriginalResponseAsync();
        
        var description = new StringBuilder()
            .Append("**Sent: **")
            .AppendLine(audit.SentTime.ToString("G"))
            .Append("**Requested by: **")
            .AppendLine(MentionUtils.MentionUser(audit.RequestedBy))
            .AppendLine()
            .Append("**Status: **")
            .AppendLine(audit.Status.ToString())
            .Append("**Completed by: **")
            .AppendLine(audit.CompletedBy == 0 ? "Pending" : MentionUtils.MentionUser(audit.CompletedBy))
            .Append("**Completed at: **")
            .AppendLine(audit.CompletedAt?.ToString("G") ?? "Pending")
            .AppendLine()
            .AppendLine("**Messages:** ");

        foreach (var message in audit.Messages)
        {
            description.Append("Message to ").Append(MentionUtils.MentionUser(message.UserId))
                .Append(": ").Append(message.Status).Append(" (").Append(message.Log).AppendLine(")");
        }
        
        description.AppendLine().AppendLine("**Log:**");
        foreach (var log in audit.Logs)
            description.Append(log.Date.ToString("G"))
                .Append(": ")
                .AppendLine(log.Message);
        
        var embed = new EmbedBuilder()
            .WithTitle($"Audit for {venue.Name}")
            .WithDescription(description.ToString());

        
        if (audit.Status.IsResponded() || !this._authorizer.Authorize(user, Permission.EditVenue, venue).Authorized)
        {
            await context.Interaction.Channel.SendMessageAsync("Okay, here's the audit! 🥰", embed: embed.Build());
            return;
        }

        var selectMenuBuilder = new SelectMenuBuilder()
            .WithValueHandlers()
            .WithPlaceholder("Answer audit");
        
        if (audit.Status is VenueAuditStatus.Failed or VenueAuditStatus.Pending)
            selectMenuBuilder
                .AddOption(new SelectMenuOptionBuilder()
                    .WithLabel("Retry")
                    .WithEmote(new Emoji("🔄"))
                    .WithDescription("Retry sending this audit to the venue's (current) managers.")
                    .WithStaticHandler(AuditHandler.Key, venue.Id, "false", audit.id));
                
        selectMenuBuilder
            .AddOption(new SelectMenuOptionBuilder()
                .WithLabel("Confirm Correct")
                .WithEmote(new Emoji("👍"))
                .WithDescription("Confirm the details on this venue are correct.")
                .WithStaticHandler(ConfirmCorrectHandler.Key, audit.id, "not-via-audit-message"))
            .AddOption(new SelectMenuOptionBuilder()
                .WithLabel("Edit Venue")
                .WithEmote(new Emoji("✏️"))
                .WithDescription("Update the details on this venue.")
                .WithStaticHandler(EditVenueHandler.Key, audit.id, "not-via-audit-message"))
            .AddOption(new SelectMenuOptionBuilder()
                .WithLabel("Temporarily Close")
                .WithEmote(new Emoji("🔒"))
                .WithDescription("Put this venue on a hiatus for up to 3 months.")
                .WithStaticHandler(TemporarilyClosedHandler.Key, audit.id, "not-via-audit-message"))
            .AddOption(new SelectMenuOptionBuilder()
                .WithLabel("Permanently Close / Delete")
                .WithEmote(new Emoji("❌"))
                .WithDescription("Delete this venue completely.")
                .WithStaticHandler(PermanentlyClosedHandler.Key, audit.id, "not-via-audit-message"))
            .AddOption(new SelectMenuOptionBuilder()
                .WithLabel("Don't answer")
                .WithStaticHandler(DismissHandler.Key));
        
        var component = new ComponentBuilder()
            .WithSelectMenu(selectMenuBuilder);
        
        await context.Interaction.Channel.SendMessageAsync("Okay, here's the audit! 🥰", embed: embed.Build(), components: component.Build());
    }
    
}