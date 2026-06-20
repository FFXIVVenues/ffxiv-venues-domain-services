using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.LocationEntry;

class IsSubdivisionEntrySessionState : ISessionState
{
    public Task Enter(VeniInteractionContext c)
    {
        return c.Interaction.RespondAsync(MessageRepository.AskForSubdivisionMessage.PickRandom(), new ComponentBuilder()
            .WithBackButton(c)
            .WithButton("Yes, it's subdivision", c.Session.RegisterComponentHandler(cm =>
            {
                var venue = cm.Session.GetVenue();
                venue.Location.Subdivision = true;
                return cm.Session.MoveStateAsync<ApartmentEntrySessionState>(cm);
            }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
            .WithButton("No, the first division", c.Session.RegisterComponentHandler(cm =>
            {
                var venue = cm.Session.GetVenue();
                venue.Location.Subdivision = false;
                return cm.Session.MoveStateAsync<ApartmentEntrySessionState>(cm);
            }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
            .Build());
    }
}