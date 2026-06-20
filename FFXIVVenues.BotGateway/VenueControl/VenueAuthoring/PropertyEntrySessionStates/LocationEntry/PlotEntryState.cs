using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.LocationEntry;

class PlotEntrySessionState : ISessionState
{
    public Task Enter(VeniInteractionContext c)
    {
        c.Session.RegisterMessageHandler(this.OnMessageReceived);
        var isDm = c.Interaction.Channel is IDMChannel;
        return c.Interaction.RespondAsync(MessageRepository.AskForPlotMessage.PickRandom(),
            new ComponentBuilder().WithBackButton(c).Build(),
            isDm ? null : new EmbedBuilder()
                .WithDescription("**@ Veni Ki** with your plot number")
                .WithColor(Color.Blue)
                .Build());
    }

    public Task OnMessageReceived(MessageVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        var match = new Regex("\\b\\d+\\b").Match(c.Interaction.Content.StripMentions());

        if (!match.Success || !ushort.TryParse(match.Value, out var plot) || plot < 1 || plot > 60)
            return c.Interaction.Channel.SendMessageAsync("Sorry, I didn't understand that, please enter a number between 1 and 60.");

        venue.Location.Room = 0;
        venue.Location.Apartment = 0;
        venue.Location.Plot = plot;
        venue.Location.Subdivision = plot > 30;

        var locationType = c.Session.GetItem<string>(SessionKeys.LOCATION_TYPE);
        if (locationType == "room")
            return c.Session.MoveStateAsync<RoomEntrySessionState>(c);

        if (c.Session.InEditing())
            return c.Session.MoveStateAsync<ConfirmVenueSessionState>(c);

        return c.Session.MoveStateAsync<SfwEntrySessionState>(c);
    }
}