using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.LocationEntry;

class HousingDistrictEntrySessionState : ISessionState
{
    public Task Enter(VeniInteractionContext c)
    {
        var districts = new[] { "Mist", "Empyreum", "Goblet", "Lavender Beds", "Shirogane" }
            .Select(zone => new SelectMenuOptionBuilder(zone, zone)).ToList();
        var selectMenu = new SelectMenuBuilder();
        selectMenu.WithOptions(districts);
        selectMenu.WithCustomId(c.Session.RegisterComponentHandler(Handle, ComponentPersistence.ClearRow));


        return c.Interaction.RespondAsync(MessageRepository.AskForHousingDistrictMessage.PickRandom(), 
            new ComponentBuilder().WithSelectMenu(selectMenu).WithBackButton(c).Build());
    }

    public Task Handle(ComponentVeniInteractionContext c)
    {
        var district = c.Interaction.Data.Values.Single();
        var venue = c.Session.GetVenue();
        venue.Location.District = district;
        return c.Session.MoveStateAsync<WardEntrySessionState>(c);
    }

}