using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;

namespace FFXIVVenues.Veni.VenueControl.VenueOpening.SessionStates;

internal class EndCurrentClosureState(IApiService apiService, IAuthorizer authorizer) : ISessionState
{
    public async Task Enter(VeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        var authorize = authorizer.Authorize(c.Interaction.User.Id, Permission.OpenVenue, venue);
        if (!authorize.Authorized)
        {
            await c.Interaction.Channel.SendMessageAsync(
                "Sorry, you do not have permission to close this venue. 😢");
            return;
        }
        
        var closure = venue.ScheduleOverrides.FirstOrDefault(s => s.IsNow && s.Open is false);
        if (closure is not null)
            await apiService.RemoveOverridesAsync(venue.Id, closure.Start, closure.End);
        await c.Interaction.Channel.SendMessageAsync(VenueControlStrings.VenueClosureEnded);
        await c.Session.ClearStateAsync(c);
    }
}

