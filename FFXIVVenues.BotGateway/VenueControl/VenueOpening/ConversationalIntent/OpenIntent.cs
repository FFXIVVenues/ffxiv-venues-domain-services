using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.VenueControl.VenueOpening.SessionStates;

namespace FFXIVVenues.Veni.VenueControl.VenueOpening.ConversationalIntent
{
    internal class OpenIntent(IApiService apiService) : IntentHandler
    {
        // todo: change to stateless handlers (like edit)
        public override async Task Handle(VeniInteractionContext context)
        {
            var user = context.Interaction.User.Id;
            var venues = await apiService.GetAllVenuesAsync(user);

            if (venues == null || !venues.Any())
                await context.Interaction.RespondAsync("You don't seem to be an assigned manager for any venues. 🤔");
            else if (venues.Count() > 1)
            {
                if (venues.Count() > 25)
                    venues = venues.Take(25);
                context.Session.SetItem(SessionKeys.VENUES, venues);
                await context.Session.MoveStateAsync<SelectVenueToOpenSessionState>(context);
            }
            else
            {
                context.Session.SetVenue(venues.First());
                await context.Session.MoveStateAsync<OpenEntryState>(context);
            }
        }

    }
}
