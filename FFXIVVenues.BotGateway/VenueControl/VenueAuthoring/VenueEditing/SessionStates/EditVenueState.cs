using System.Threading.Tasks;
using Discord;
using FFXIVVenues.BotGateway.Authorisation;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Context.SessionHandling;
using FFXIVVenues.BotGateway.Utils;
using FFXIVVenues.BotGateway.VenueControl;
using FFXIVVenues.BotGateway.VenueRendering;

namespace FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.VenueEditing.SessionStates
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
