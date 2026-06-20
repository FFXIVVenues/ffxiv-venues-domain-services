using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.LocationEntry;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates;

class DescriptionEntrySessionState : ISessionState
{
    public Task Enter(VeniInteractionContext c)
    {
        c.Session.RegisterMessageHandler(this.OnMessageReceived);
        var isDm = c.Interaction.Channel is IDMChannel;
        return c.Interaction.RespondAsync(VenueControlStrings.AskForDescriptionMessage,
            new ComponentBuilder()
                .WithBackButton(c)
                .WithSkipButton<LocationTypeEntrySessionState, ConfirmVenueSessionState>(c)
                .Build(),
            isDm ? null : new EmbedBuilder()
                .WithDescription("**@ Veni Ki** with your description")
                .WithColor(Color.Blue)
                .Build());
    }

    public Task OnMessageReceived(MessageVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        venue.Description = c.Interaction.Content.StripMentions().AsListOfParagraphs();
        if (c.Session.InEditing())
            return c.Session.MoveStateAsync<ConfirmVenueSessionState>(c);
        return c.Session.MoveStateAsync<LocationTypeEntrySessionState>(c);
    }

}