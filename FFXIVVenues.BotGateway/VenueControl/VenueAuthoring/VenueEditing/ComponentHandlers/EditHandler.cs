using System.Threading.Tasks;
using Discord;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Authorisation;
using FFXIVVenues.BotGateway.Infrastructure.Components;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Utils;
using FFXIVVenues.BotGateway.VenueRendering;

namespace FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.VenueEditing.ComponentHandlers;

public class EditHandler(IAuthorizer authorizer, IApiService apiService, IVenueRenderer venueRenderer)
    : IComponentHandler
{

    // Change this key and any existing buttons linked to this will die
    public static string Key => "CONTROL_EDIT";

    public async Task HandleAsync(ComponentVeniInteractionContext context, string[] args)
    {
        var user = context.Interaction.User.Id;
        var venueId = args[0];
        var venue = await apiService.GetVenueAsync(venueId);
        if (! authorizer.Authorize(user, Permission.EditVenue, venue).Authorized)
            return;
        
        _ = context.Interaction.ModifyOriginalResponseAsync(props =>
            props.Components = new ComponentBuilder().Build());
        
        await context.Interaction.Channel.SendMessageAsync(MessageRepository.EditVenueMessage.PickRandom(),
            components: venueRenderer.RenderEditComponents(venue, context.Interaction.User.Id).Build());
    }
    
}