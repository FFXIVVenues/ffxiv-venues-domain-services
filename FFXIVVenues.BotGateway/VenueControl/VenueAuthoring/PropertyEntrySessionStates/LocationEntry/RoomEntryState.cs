using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.LocationEntry;

class RoomEntrySessionState : ISessionState
{
    public Task Enter(VeniInteractionContext c)
    {
        c.Session.RegisterMessageHandler(this.OnMessageReceived);
        return c.Interaction.RespondAsync(MessageRepository.AskForRoomMessage.PickRandom(),
            new ComponentBuilder().WithBackButton(c).Build());
    }

    public Task OnMessageReceived(MessageVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        var match = new Regex("\\b\\d+\\b").Match(c.Interaction.Content.StripMentions());

        if (!match.Success || !ushort.TryParse(match.Value, out var room) || room < 1)
            return c.Interaction.Channel.SendMessageAsync("Sorry, I didn't understand that, please enter your room number.");

        venue.Location.Room = room;

        if (c.Session.InEditing())
            return c.Session.MoveStateAsync<ConfirmVenueSessionState>(c);

        return c.Session.MoveStateAsync<SfwEntrySessionState>(c);
    }
}