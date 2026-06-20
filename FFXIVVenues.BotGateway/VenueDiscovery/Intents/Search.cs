using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Intent;
using FFXIVVenues.BotGateway.VenueControl;
using FFXIVVenues.BotGateway.VenueDiscovery.SessionStates;
using FFXIVVenues.BotGateway.VenueRendering;

namespace FFXIVVenues.BotGateway.VenueDiscovery.Intents;

internal class Search(IApiService apiService, IVenueRenderer venueRenderer) : IntentHandler
{
    public override async Task Handle(VeniInteractionContext context)
    {
        var asker = context.Interaction.User.Id;
        var query = context.Prediction?.Entities.FirstOrDefault(e => e.Category == "search-query")?.Text;

        if (string.IsNullOrWhiteSpace(query))
        {
            await context.Interaction.RespondAsync("What am I looking for? 🤔");
            return;
        }

        var venues = await apiService.GetAllVenuesAsync(query);

        if (venues == null || !venues.Any())
            await context.Interaction.RespondAsync("Could find any venues with that name. 😔");
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