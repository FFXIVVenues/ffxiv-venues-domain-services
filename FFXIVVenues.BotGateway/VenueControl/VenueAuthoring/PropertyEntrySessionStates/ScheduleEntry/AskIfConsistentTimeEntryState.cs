using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.ScheduleEntry
{
    class AskIfConsistentTimeEntrySessionState : ISessionState
    {

        private static string[] _messages = new[]
        {
            "Is the venue **open the same time** across each these days?",
            "Is the scheduled **opening time the same** across all those days?"
        };

        public Task Enter(VeniInteractionContext c) =>
            c.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {_messages.PickRandom()}",
                new ComponentBuilder()
                    .WithBackButton(c)
                    .WithButton("Yes, each day has the same opening/closing time",
                        c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<ConsistentOpeningTimeEntrySessionState>(cm), 
                    ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("No, it's different opening times between days",
                        c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<InconsistentOpeningTimeEntrySessionState>(cm), 
                    ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .Build());

    }
}
