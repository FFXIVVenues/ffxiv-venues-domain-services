using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueControl;
using FFXIVVenues.Veni.VenueRendering;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueDiscovery.SessionStates
{
    class SelectVenueToShowSessionState(
        IVenueRenderer venueRenderer)
        : ISessionState
    {
        
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
            return c.Interaction.RespondAsync(MessageRepository.ShowVenueResponses.PickRandom(), componentBuilder.Build());
        }

        public async Task Handle(ComponentVeniInteractionContext context)
        {
            var selectedVenueId = context.Interaction.Data.Values.Single();
            var asker = context.Interaction.User.Id;
            var venue = _managersVenues.FirstOrDefault(v => v.Id == selectedVenueId);

            await context.Session.ClearStateAsync(context);

            var render = await venueRenderer.ValidateAndRenderAsync(venue);
            await context.Interaction.FollowupAsync(embed: render.Build(),
                components: venueRenderer.RenderActionComponents(context, venue, asker).Build());
        }

        
    }
}
