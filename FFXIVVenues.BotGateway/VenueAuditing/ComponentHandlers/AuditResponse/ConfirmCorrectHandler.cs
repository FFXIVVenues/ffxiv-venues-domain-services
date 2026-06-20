using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueAuditing.ComponentHandlers.AuditResponse;

public class ConfirmCorrectHandler(
    IRepository repository,
    IApiService apiService,
    IAuthorizer authorizer,
    IVenueAuditService auditService)
    : BaseAuditHandler
{
    
    // Change this key and any existing buttons linked to this will die
    public static string Key => "AUDIT_CONFIRM_CORRECT";

    private readonly string[] _responses = new[]
    {
        "Thanks! 🥰",
        "Thank you! 🥰",
        "Thankies! 💕",
        "Thank you so much! 💕"
    };

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
        
        await context.Interaction.Message.Channel.SendMessageAsync(_responses.PickRandom());
        
        await auditService.UpdateAuditStatus(audit, venue, context.Interaction.User.Id,
            VenueAuditStatus.RespondedConfirmed);
        
        if (audit.Messages.All(m => m.MessageId != context.Interaction.Message.Id))
            await context.Interaction.ModifyOriginalResponseAsync(m => m.Components = new ComponentBuilder().Build());
    }
    
}