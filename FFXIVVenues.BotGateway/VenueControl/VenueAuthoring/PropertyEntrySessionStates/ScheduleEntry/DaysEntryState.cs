using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.ScheduleEntry
{
    class DaysEntrySessionState : ISessionState
    {

        private static List<(string Label, Day Value)> _availableDays = new()
        {
            ("Monday", Day.Monday),
            ("Tuesday", Day.Tuesday),
            ("Wednesday", Day.Wednesday),
            ("Thursday", Day.Thursday),
            ("Friday", Day.Friday),
            ("Saturday", Day.Saturday),
            ("Sunday", Day.Sunday),
        };

        private Venue _venue;

        public Task Enter(VeniInteractionContext c)
        {
            this._venue = c.Session.GetVenue();

            var component = this.BuildDaysComponent(c).WithBackButton(c);
            if (this._venue.Schedule.Count > 1)
                component.WithSkipButton<AskIfConsistentTimeEntrySessionState, AskIfConsistentTimeEntrySessionState>(c);
            else if (this._venue.Schedule.Count == 1)
                component.WithSkipButton<ConsistentOpeningTimeEntrySessionState, ConsistentOpeningTimeEntrySessionState>(c);

            return c.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {MessageRepository.AskDaysOpenMessage.PickRandom()}", component: component.Build());
        }

        private ComponentBuilder BuildDaysComponent(VeniInteractionContext c)
        {
            var selectComponent = new SelectMenuBuilder()
                .WithCustomId(c.Session.RegisterComponentHandler(OnComplete, ComponentPersistence.ClearRow))
                .WithMinValues(1)
                .WithMaxValues(_availableDays.Count);
            foreach (var (label, value) in _availableDays)
                selectComponent.AddOption(label, value.ToString(), isDefault: this._venue.Schedule.Any(o => o.Day == value));

            return new ComponentBuilder().WithSelectMenu(selectComponent);
        }

        private Task OnComplete(ComponentVeniInteractionContext c)
        {
            this._venue.Schedule = c.Interaction.Data.Values
                                    .Select(d => new Schedule { Day = Enum.Parse<Day>(d) })
                                    .ToList();

            if (this._venue.Schedule.Count > 1)
                return c.Session.MoveStateAsync<AskIfConsistentTimeEntrySessionState>(c);

            return c.Session.MoveStateAsync<ConsistentOpeningTimeEntrySessionState>(c);
        }

    }
}
