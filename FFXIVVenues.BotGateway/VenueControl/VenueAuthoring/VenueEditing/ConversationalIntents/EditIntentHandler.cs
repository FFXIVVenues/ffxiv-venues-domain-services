using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Intent;
using FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.VenueEditing.ComponentHandlers;
using FFXIVVenues.BotGateway.VenueRendering;

namespace FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.VenueEditing.ConversationalIntents;

internal class EditIntentHandler(IApiService apiService, IVenueRenderer venueRenderer) : IntentHandler
{
    public override async Task Handle(VeniInteractionContext context)
    {
        var user = context.Interaction.User.Id;
        var venues = await apiService.GetAllVenuesAsync(user);

        if (venues == null || !venues.Any())
        {
            await context.Interaction.RespondAsync("You don't seem to be an assigned manager for any venues. 🤔");
            return;
        }

        await context.Session.ClearStateAsync(context);
        
        // ReSharper disable once PossibleMultipleEnumeration
        // Enumerating next once for the Any is better than enumerating all on a chance
        venues = venues.ToList();
        if (venues.Count() == 1)
        {
            var venue = venues.Single();
            await context.Interaction.RespondAsync(embed: (await venueRenderer.ValidateAndRenderAsync(venue)).Build(),
                component: venueRenderer.RenderEditComponents(venue, user).Build());
            return;
        }
                
        if (venues.Count() > 25)
            venues = venues.Take(25);
                
        await context.Interaction.RespondAsync(VenueControlStrings.SelectVenueToEdit,
            component: venueRenderer.RenderVenueSelection(venues, SelectVenueToEditHandler.Key).Build());
    }

}