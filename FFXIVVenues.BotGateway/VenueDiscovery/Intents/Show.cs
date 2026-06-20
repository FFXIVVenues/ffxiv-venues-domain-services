using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Intent;
using FFXIVVenues.BotGateway.VenueControl;
using FFXIVVenues.BotGateway.VenueDiscovery.SessionStates;
using FFXIVVenues.BotGateway.VenueRendering;

namespace FFXIVVenues.BotGateway.VenueDiscovery.Intents;

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