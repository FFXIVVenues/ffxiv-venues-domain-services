using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Intent;
using FFXIVVenues.BotGateway.VenueControl;
using FFXIVVenues.BotGateway.VenueControl.VenueClosing.SessionStates;

namespace FFXIVVenues.BotGateway.VenueControl.VenueClosing.ConversationalIntent
{
    internal class CloseIntent(IApiService apiService) : IntentHandler
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
                await context.Session.MoveStateAsync<SelectVenueToCloseSessionState>(context);
            }
            else
            {
                context.Session.SetVenue(venues.First());
                await context.Session.MoveStateAsync<CloseEntryState>(context);
            }
        }

    }
}