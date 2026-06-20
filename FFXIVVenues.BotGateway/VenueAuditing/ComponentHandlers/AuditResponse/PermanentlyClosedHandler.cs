using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.VenueControl;
using FFXIVVenues.Veni.VenueControl.VenueDeletion.SessionStates;

namespace FFXIVVenues.Veni.VenueAuditing.ComponentHandlers.AuditResponse;

public class PermanentlyClosedHandler(
    IRepository repository,
    IApiService apiService,
    IAuthorizer authorizer,
    IVenueAuditService venueAuditService)
    : BaseAuditHandler
{

    // Change this key and any existing buttons linked to this will die
    public static string Key => "AUDIT_PERM_CLOSED";

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
        
        context.Session.SetVenue(venue);
        await context.Session.MoveStateAsync<DeleteVenueSessionState>(context);

        await venueAuditService.UpdateAuditStatus(audit, venue, context.Interaction.User.Id,
            VenueAuditStatus.RespondedDelete);
       
        if (audit.Messages.All(m => m.MessageId != context.Interaction.Message.Id))
            await context.Interaction.ModifyOriginalResponseAsync(m => m.Components = new ComponentBuilder().Build());
    }
    
}