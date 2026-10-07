using System.Threading.Tasks;
using Discord;
using FFXIVVenues.BotGateway.Authorisation;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Context.SessionHandling;
using FFXIVVenues.BotGateway.Utils;
using FFXIVVenues.BotGateway.VenueControl;
using FFXIVVenues.BotGateway.VenueRendering;

namespace FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.VenueEditing.SessionStates;

class EditVenueSessionState(IAuthorizer authorizer, DtoVenueRenderer venueRenderer) : ISessionState
{
    private readonly IAuthorizer _authorizer = authorizer;

    public Task Enter(VeniInteractionContext c)
    {
        c.Session.SetEditing(true);
        var venue = c.Session.GetVenue();

        return c.Interaction.RespondAsync(MessageRepository.EditVenueMessage.PickRandom(),
            component: venueRenderer.RenderEditComponents(venue, c.Interaction.User.Id).Build());
    }

}