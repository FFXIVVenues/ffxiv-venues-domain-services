using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Authorisation;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.BotGateway.VenueControl;
using FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.VenueEditing.SessionStates;

namespace FFXIVVenues.BotGateway.VenueAuditing.ComponentHandlers.AuditResponse;

public class EditVenueHandler(
    IRepository repository,
    IApiService apiService,
    IAuthorizer authorizer,
    IVenueAuditService auditService)
    : BaseAuditHandler
{

    // Change this key and any existing buttons linked to this will die
    public static string Key => "AUDIT_EDIT_VENUE";

    public override async Task HandleAsync(ComponentVeniInteractionContext context, string[] args)
    {
        var auditId = args[0];
        var audit = await repository.GetByIdAsync<VenueAuditRecord>(auditId);
        var venue = await apiService.GetVenueAsync(audit.VenueId);
        
        if (!authorizer.Authorize(context.Interaction.User.Id, Permission.EditVenue, venue).Authorized)
        {
            await context.Interaction.Message.Channel.SendMessageAsync("Sorry, I can't let you do that. 🥲");
            return;
        }
        
        await auditService.UpdateAuditStatus(audit, venue, context.Interaction.User.Id,
            VenueAuditStatus.RespondedEdit);
        
        context.Session.SetVenue(venue);
        await context.Session.MoveStateAsync<EditVenueSessionState>(context);
        
        if (audit.Messages.All(m => m.MessageId != context.Interaction.Message.Id))
            await context.Interaction.ModifyOriginalResponseAsync(m => m.Components = new ComponentBuilder().Build());
    }
    
}