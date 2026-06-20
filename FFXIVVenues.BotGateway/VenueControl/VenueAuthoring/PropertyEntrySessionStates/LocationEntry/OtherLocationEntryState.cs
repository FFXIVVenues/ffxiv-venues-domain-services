using System.Threading.Tasks;
using Discord;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Context.SessionHandling;
using FFXIVVenues.BotGateway.Utils;
using FFXIVVenues.BotGateway.VenueControl;
using FFXIVVenues.BotGateway.VenueControl.VenueAuthoring;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.PropertyEntrySessionStates.LocationEntry;

internal class OtherLocationEntrySessionState : ISessionState
{
    
    public Task Enter(VeniInteractionContext c)
    {
        c.Session.RegisterMessageHandler(this.MessageHandler);
        return c.Interaction.RespondAsync("Ooo, interesting! In as few characters as possible, where is your venue **located**? 🥰", new ComponentBuilder()
            .WithBackButton(c).Build());
    }

    private Task MessageHandler(MessageVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        venue.Location = new Location { Override = c.Interaction.Content.StripMentions() };
            
        if (c.Session.InEditing())
            return c.Session.MoveStateAsync<ConfirmVenueSessionState>(c);

        return c.Session.MoveStateAsync<SfwEntrySessionState>(c);
    }
    
}