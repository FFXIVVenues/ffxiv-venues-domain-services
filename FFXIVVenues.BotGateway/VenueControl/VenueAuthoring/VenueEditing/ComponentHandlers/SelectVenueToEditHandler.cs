using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Authorisation;
using FFXIVVenues.BotGateway.Infrastructure.Components;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.VenueControl;
using FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.VenueEditing.SessionStates;
using FFXIVVenues.BotGateway.VenueRendering;

namespace FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.VenueEditing.ComponentHandlers;

public class SelectVenueToEditHandler(IAuthorizer authorizer, IApiService apiService, IVenueRenderer venueRenderer)
    : IComponentHandler
{
    public static string Key => "CONTROL_SELECT_EDIT";

    private readonly IVenueRenderer _venueRenderer = venueRenderer;

    public async Task HandleAsync(ComponentVeniInteractionContext context, string[] args)
    {
        var user = context.Interaction.User.Id;
        var venueId = context.Interaction.Data.Values.First();
        var venue = await apiService.GetVenueAsync(venueId);
        if (! authorizer.Authorize(user, Permission.EditVenue, venue).Authorized)
            return;
        
        _ = context.Interaction.ModifyOriginalResponseAsync(props =>
                    props.Components = new ComponentBuilder().Build());
        
        context.Session.SetVenue(venue);
        await context.Session.MoveStateAsync<EditVenueSessionState>(context);
    }
    
}