using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Authorisation;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Context.SessionHandling;
using FFXIVVenues.BotGateway.VenueControl;

namespace FFXIVVenues.BotGateway.VenueControl.VenueOpening.SessionStates;

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

