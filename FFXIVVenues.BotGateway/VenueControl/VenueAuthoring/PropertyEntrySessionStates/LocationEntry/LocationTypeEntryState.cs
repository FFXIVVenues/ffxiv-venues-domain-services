using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.LocationEntry;

class LocationTypeEntrySessionState : ISessionState
{
    private static List<(string Label, string Value, string Emote)> _options = new()
    {
        (VenueControlStrings.LocationTypeLabel_House, "house", "🏠"),
        (VenueControlStrings.LocationTypeLabel_Room, "room", "🚪"),
        (VenueControlStrings.LocationTypeLabel_Apartment, "apartment", "🏢"),
        (VenueControlStrings.LocationTypeLabel_Other, "other", "❓")
    };

    public Task Enter(VeniInteractionContext c)
    {
        var sessionValue = c.Session.GetItem<string>("locationType");
        var selectComponent = new SelectMenuBuilder()
            .WithCustomId(c.Session.RegisterComponentHandler(OnComplete, ComponentPersistence.ClearRow))
            .WithMaxValues(1);

        foreach (var (label, value, emote) in _options)
            selectComponent.AddOption(label, value, isDefault: sessionValue == value, emote: new Emoji(emote));

        return c.Interaction.RespondAsync(MessageRepository.AskForHouseOrApartmentMessage.PickRandom(), new ComponentBuilder()
            .WithSelectMenu(selectComponent)
            .WithBackButton(c)
            .Build());
    }

    private Task OnComplete(ComponentVeniInteractionContext c)
    {
        var value = c.Interaction.Data.Values.First();
        c.Session.SetItem("locationType", value);

        if (value == "other")
            return c.Session.MoveStateAsync<OtherLocationEntrySessionState>(c);

        return c.Session.MoveStateAsync<DataCenterEntrySessionState>(c);
    }
}