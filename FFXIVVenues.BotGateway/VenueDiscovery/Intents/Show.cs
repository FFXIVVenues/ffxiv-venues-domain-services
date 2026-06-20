using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.VenueControl;
using FFXIVVenues.Veni.VenueDiscovery.SessionStates;
using FFXIVVenues.Veni.VenueRendering;

namespace FFXIVVenues.Veni.VenueDiscovery.Intents;

internal class Show(IApiService apiService, IVenueRenderer venueRenderer) : IntentHandler
{
    public override async Task Handle(VeniInteractionContext context)
    {
        var asker = context.Interaction.User.Id;
        var venues = await apiService.GetAllVenuesAsync(asker);

        if (venues == null || !venues.Any())
            await context.Interaction.RespondAsync("You don't seem to be an assigned manager for any venues. 🤔");
        else if (venues.Count() > 1)
        {
            if (venues.Count() > 25)
                venues = venues.Take(25);
            context.Session.SetItem(SessionKeys.VENUES, venues);
            await context.Session.MoveStateAsync<SelectVenueToShowSessionState>(context);
        }
        else
        {
            var venue = venues.Single();
            var render = await venueRenderer.ValidateAndRenderAsync(venue);
            await context.Interaction.RespondAsync(embed: render.Build(),
                component: venueRenderer.RenderActionComponents(context, venue, asker).Build());
        }
    }

}