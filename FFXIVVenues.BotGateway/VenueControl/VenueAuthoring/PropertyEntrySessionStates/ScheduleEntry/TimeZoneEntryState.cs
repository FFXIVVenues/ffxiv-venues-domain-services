using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.ScheduleEntry;

class TimeZoneEntrySessionState : ISessionState
{

    private static string[] _messages = new[]
    {
        "What **time zone** would you like to give opening times in?",
        "What **time zone** would the venues opening times be in?"
    };

    public Task Enter(VeniInteractionContext c)
    {
        var component = new ComponentBuilder();
        var timezoneOptions = TimeZones.SupportedTimeZones.Select(dc => new SelectMenuOptionBuilder(dc.TimeZoneLabel, dc.TimeZoneKey)).ToList();
        var selectMenu = new SelectMenuBuilder();
        selectMenu.WithOptions(timezoneOptions);
        selectMenu.WithCustomId(c.Session.RegisterComponentHandler(Handle, ComponentPersistence.ClearRow));
            
        return c.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} { _messages.PickRandom()}",
            component.WithSelectMenu(selectMenu).WithBackButton(c).Build());
    }

    private Task Handle(ComponentVeniInteractionContext c)
    {
        var selectedTimezone = c.Interaction.Data.Values.Single();
        c.Session.SetItem(SessionKeys.TIMEZONE_ID, selectedTimezone);
        return c.Session.MoveStateAsync<DaysEntrySessionState>(c);
    }
}