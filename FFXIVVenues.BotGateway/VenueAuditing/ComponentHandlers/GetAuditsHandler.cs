using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Authorisation;
using FFXIVVenues.BotGateway.Infrastructure.Components;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Persistence.Abstraction;

namespace FFXIVVenues.BotGateway.VenueAuditing.ComponentHandlers;

public class GetAuditsHandler(IAuthorizer authorizer, IRepository repository, IApiService apiService)
    : IComponentHandler
{

    // Change this key and any existing buttons linked to this will die
    public static string Key => "CONTROL_GET_AUDITS";

    public async Task HandleAsync(ComponentVeniInteractionContext context, string[] args)
    {
        var user = context.Interaction.User.Id;
        var venueId = args[0];
        var venue = await apiService.GetVenueAsync(venueId);
        if (!authorizer.Authorize(user, Permission.ViewAuditHistory, venue).Authorized)
            return;
        
        _ = context.Interaction.ModifyOriginalResponseAsync(props =>
            props.Components = new ComponentBuilder().Build());
        
        var auditsQuery = await repository.GetWhereAsync<VenueAuditRecord>(r => r.VenueId == venueId);
        var audits = auditsQuery.ToList();
        
        if (!audits.Any())
        {
            await context.Interaction.Channel.SendMessageAsync("No audits on record for this venue yet! 🥰");
            return;
        }
        
        var builder = new ComponentBuilder();
        var dropDown = new SelectMenuBuilder()
            .WithStaticHandler(GetAuditHandler.Key)
            .WithPlaceholder("What would you like to do?");

        foreach (var audit in audits.OrderByDescending(a => a.SentTime))
            dropDown.AddOption(
                audit.MassAuditId is not null
                    ? $"Mass audit sent at {audit.SentTime:G}"
                    : $"Audit sent at {audit.SentTime:G}", audit.id, $"Status: {audit.Status}");

        builder.WithSelectMenu(dropDown);
        await context.Interaction.Channel.SendMessageAsync("Okay, here they are! 🥰", components: builder.Build());
    }
    
}