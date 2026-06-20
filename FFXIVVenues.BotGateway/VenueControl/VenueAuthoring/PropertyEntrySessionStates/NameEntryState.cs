using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates;

class NameEntrySessionState : ISessionState
{
    public Task Enter(VeniInteractionContext c)
    {
        c.Session.RegisterMessageHandler(this.OnMessageReceived);
        var isDm = c.Interaction.Channel is IDMChannel;

        return c.Interaction.RespondAsync(VenueControlStrings.AskForNameMessage,
            embed: isDm ? null: new EmbedBuilder()
                .WithDescription("**@ Veni Ki** with your venue name")
                .WithColor(Color.Blue)
                .Build());
    }

    public Task OnMessageReceived(MessageVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        venue.Name = c.Interaction.Content.StripMentions();
        if (c.Session.InEditing())
            return c.Session.MoveStateAsync<ConfirmVenueSessionState>(c);
        return c.Session.MoveStateAsync<DescriptionEntrySessionState>(c);
    }
}