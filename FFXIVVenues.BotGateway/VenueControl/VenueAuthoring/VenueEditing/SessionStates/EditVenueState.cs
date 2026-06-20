using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueRendering;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueEditing.SessionStates
{
    class EditVenueSessionState : ISessionState
    {
        private readonly IAuthorizer _authorizer;
        private readonly IVenueRenderer _venueRenderer;

        public EditVenueSessionState(IAuthorizer authorizer, IVenueRenderer venueRenderer)
        {
            this._authorizer = authorizer;
            this._venueRenderer = venueRenderer;
        }

        public Task Enter(VeniInteractionContext c)
        {
            c.Session.SetEditing(true);
            var venue = c.Session.GetVenue();

            if (c.Interaction.IsDM)
                return c.Interaction.RespondAsync(MessageRepository.EditVenueMessage.PickRandom(), 
                    component: this._venueRenderer.RenderEditComponents(venue, c.Interaction.User.Id).Build());

            var @warningEmbed = new EmbedBuilder
            {
                Color = Color.Red,
                Description = MessageRepository.MentionOrReplyToMeMessage.PickRandom()
            };
            return c.Interaction.RespondAsync(MessageRepository.EditVenueMessage.PickRandom(),
              embed: @warningEmbed.Build(),
              component: this._venueRenderer.RenderEditComponents(venue, c.Interaction.User.Id).Build());
        }

    }
}
