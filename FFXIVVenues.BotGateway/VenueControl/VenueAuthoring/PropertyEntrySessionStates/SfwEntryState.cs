using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.TagsEntry;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates
{
    class SfwEntrySessionState : ISessionState
    {
        private static List<(string Label, string Description, bool Value, string Emote)> _options = new()
        {
            (VenueControlStrings.SfwLabel_True, VenueControlStrings.SfwDescription_True, true, "✅"),
            (VenueControlStrings.SfwLabel_False, VenueControlStrings.SfwDescription_False, false, "🔞")
        };

        public Task Enter(VeniInteractionContext c)
        {
            var venue = c.Session.GetVenue();
            var selectComponent = new SelectMenuBuilder()
                .WithCustomId(c.Session.RegisterComponentHandler(OnComplete, ComponentPersistence.ClearRow))
                .WithMaxValues(1);

            foreach (var (label, description, value, emote) in _options)
                selectComponent.AddOption(label, value.ToString(), isDefault: venue.Sfw == value, description: description, emote: new Emoji(emote));

            return c.Interaction.RespondAsync(MessageRepository.AskForSfwMessage.PickRandom(), new ComponentBuilder()
                .WithSelectMenu(selectComponent)
                .WithBackButton(c)
                .WithSkipButton<SceneEntrySessionState, ConfirmVenueSessionState>(c)
                .Build());
        }

        private Task OnComplete(ComponentVeniInteractionContext c)
        {
            var venue = c.Session.GetVenue();
            venue.Sfw = bool.Parse(c.Interaction.Data.Values.First());

            if (c.Session.InEditing())
                return c.Session.MoveStateAsync<ConfirmVenueSessionState>(c);
            return c.Session.MoveStateAsync<SceneEntrySessionState>(c);
        }
    }
}
