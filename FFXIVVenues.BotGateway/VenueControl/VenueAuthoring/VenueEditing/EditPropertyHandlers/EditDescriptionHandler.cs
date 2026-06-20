using System.Threading.Tasks;
using Discord;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Authorisation;
using FFXIVVenues.BotGateway.Infrastructure.Components;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.VenueControl;
using FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.PropertyEntrySessionStates;

namespace FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.VenueEditing.EditPropertyHandlers;

public class EditDescriptionHandler(IAuthorizer authorizer, IApiService apiService) : IComponentHandler
{
    public static string Key => "CONTROL_EDIT_DESCRIPTION";

    public async Task HandleAsync(ComponentVeniInteractionContext context, string[] args)
    {
        var user = context.Interaction.User.Id;
        var venueId = args[0];

        var alreadyModifying = context.Session.InEditing();
        var venue = alreadyModifying ? context.Session.GetVenue() : await apiService.GetVenueAsync(venueId);
        
        if (!authorizer.Authorize(user, Permission.EditVenue, venue).Authorized)
        {
            await context.Interaction.FollowupAsync(VenueControlStrings.NoPermission);
            return;
        }

        _ = context.Interaction.ModifyOriginalResponseAsync(props =>
                    props.Components = new ComponentBuilder().Build());

        if (!alreadyModifying)
        {
            context.Session.SetVenue(venue);
            context.Session.SetEditing();
        }
        
        await context.Session.MoveStateAsync<DescriptionEntrySessionState>(context);
    }
    
}