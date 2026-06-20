using System.Threading.Tasks;
using Discord;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Authorisation;
using FFXIVVenues.BotGateway.Infrastructure.Components;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.VenueControl;
using FFXIVVenues.BotGateway.VenueControl.VenueClosing.SessionStates;

namespace FFXIVVenues.BotGateway.VenueControl.VenueClosing.ComponentHandlers;

public class CloseHandler(IAuthorizer authorizer, IApiService apiService) : IComponentHandler
{

    // Change this key and any existing buttons linked to this will die
    public static string Key => "CONTROL_CLOSE";

    public async Task HandleAsync(ComponentVeniInteractionContext context, string[] args)
    {
        var user = context.Interaction.User.Id;
        var venueId = args[0];
        var venue = await apiService.GetVenueAsync(venueId);
        
        if (!authorizer.Authorize(user, Permission.CloseVenue, venue).Authorized)
            return;
        
        _ = context.Interaction.ModifyOriginalResponseAsync(props =>
            props.Components = new ComponentBuilder().Build());
        
        context.Session.SetVenue(venue);
        await context.Session.MoveStateAsync<CloseEntryState>(context);
    }
    
}