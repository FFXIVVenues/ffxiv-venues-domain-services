using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Context.SessionHandling;
using FFXIVVenues.BotGateway.Utils;
using FFXIVVenues.BotGateway.VenueControl;

namespace FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.PropertyEntrySessionStates.LocationEntry;

class WorldEntrySessionState : ISessionState
{

    public Task Enter(VeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();

        var worlds = FfxivWorlds.GetWorldsFor(venue.Location.DataCenter)
            .Select(w => new SelectMenuOptionBuilder(w, w)).ToList();
        var selectMenu = new SelectMenuBuilder();
        selectMenu.WithOptions(worlds);
        selectMenu.WithCustomId(c.Session.RegisterComponentHandler(Handle, ComponentPersistence.ClearRow));
            
        return c.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {MessageRepository.AskForWorldMessage.PickRandom()}", 
            new ComponentBuilder().WithSelectMenu(selectMenu).WithBackButton(c).Build());
    }

    public Task Handle(ComponentVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        var world = c.Interaction.Data.Values.Single();
        venue.Location.World = world;
        return c.Session.MoveStateAsync<HousingDistrictEntrySessionState>(c);
    }

}