using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Context.SessionHandling;
using FFXIVVenues.BotGateway.Utils;
using FFXIVVenues.BotGateway.VenueControl;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Authorisation;
using FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.PropertyEntrySessionStates.ScheduleEntry;

namespace FFXIVVenues.BotGateway.VenueControl.VenueOpening.SessionStates;

internal class OpenTimeZoneEntryState : ISessionState
{
       public Task Enter(VeniInteractionContext c)
       {
           var component = new ComponentBuilder();
           var timezoneOptions = TimeZones.SupportedTimeZones.Select(dc => new SelectMenuOptionBuilder(dc.TimeZoneLabel, dc.TimeZoneKey)).ToList();
           var selectMenu = new SelectMenuBuilder();
           selectMenu.WithOptions(timezoneOptions);
           selectMenu.WithCustomId(c.Session.RegisterComponentHandler(Handle, ComponentPersistence.ClearRow));
               
           return c.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} { VenueControlStrings.AskForOpeningTimeZone}",
               component.WithSelectMenu(selectMenu).WithBackButton(c).Build());
       }
   
       private Task Handle(ComponentVeniInteractionContext c)
       {
           var selectedTimezone = c.Interaction.Data.Values.Single();
           c.Session.SetItem(SessionKeys.TIMEZONE_ID, selectedTimezone);
           return c.Session.MoveStateAsync<OpenDayEntryState>(c);
       }
}

