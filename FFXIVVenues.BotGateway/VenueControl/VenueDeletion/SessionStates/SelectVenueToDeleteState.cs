using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueDeletion.SessionStates
{
    class SelectVenueToDeleteSessionState : ISessionState
    {

        private static string[] _messages = new[]
        {
            "Oh noes! 😥\nWhich venue would you like to delete?",
            "Sadge 😥\nWhich one are we deleting?",
            "😥 Which one?"
        };

        private IEnumerable<Venue> _managersVenues;

        public Task Enter(VeniInteractionContext c)
        {
            _managersVenues = c.Session.GetItem<IEnumerable<Venue>>(SessionKeys.VENUES);

            var selectMenuKey = c.Session.RegisterComponentHandler(this.Handle, ComponentPersistence.DeleteMessage);
            var componentBuilder = new ComponentBuilder();
            var selectMenuBuilder = new SelectMenuBuilder() { CustomId = selectMenuKey };
            foreach (var venue in _managersVenues.OrderBy(v => v.Name))
            {
                var selectMenuOption = new SelectMenuOptionBuilder
                {
                    Label = venue.Name,
                    Description = venue.Location.ToString(),
                    Value = venue.Id
                };
                selectMenuBuilder.AddOption(selectMenuOption);
            }
            componentBuilder.WithSelectMenu(selectMenuBuilder);
            return c.Interaction.RespondAsync(_messages.PickRandom(), componentBuilder.Build());
        }

        public Task Handle(ComponentVeniInteractionContext c)
        {
            var selectedVenueId = c.Interaction.Data.Values.Single();
            var venue = _managersVenues.FirstOrDefault(v => v.Id == selectedVenueId);

            c.Session.ClearItem(SessionKeys.VENUES);
            c.Session.SetVenue(venue);

            return c.Session.MoveStateAsync<DeleteVenueSessionState>(c);
        }
    }
}
