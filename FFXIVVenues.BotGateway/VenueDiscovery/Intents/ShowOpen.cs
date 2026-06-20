using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Context.SessionHandling;
using FFXIVVenues.BotGateway.Infrastructure.Intent;
using FFXIVVenues.BotGateway.Utils;
using FFXIVVenues.BotGateway.VenueRendering;
using FFXIVVenues.VenueModels;
using MomentNet.Display;

namespace FFXIVVenues.BotGateway.VenueDiscovery.Intents
{
    internal class ShowOpen : IntentHandler
    {

        private readonly IApiService _apiService;
        private readonly IVenueRenderer _venueRenderer;
        private IEnumerable<Venue> _venues;

        public ShowOpen(IApiService apiService,
                        IVenueRenderer venueRenderer)
        {
            this._apiService = apiService;
            this._venueRenderer = venueRenderer;
        }

        // todo: change to stateless handlers (like edit)
        public override async Task Handle(VeniInteractionContext c)
        {
            var asker = c.Interaction.User.Id;
            this._venues = await this._apiService.GetOpenVenuesAsync();

            if (this._venues == null || !this._venues.Any())
            {
                await c.Interaction.RespondAsync("There are no venues open at the moment. 🤔");
                return;
            }

            var venueModels = this._venues
                .OrderBy(v => v.Resolution.Start)
                .Take(25);

            var selectMenuKey = c.Session.RegisterComponentHandler(this.HandleVenueSelection, ComponentPersistence.PersistRow);
            var componentBuilder = new ComponentBuilder();
            var selectMenuBuilder = new SelectMenuBuilder() { CustomId = selectMenuKey };
            foreach (var venue in venueModels)
            {
                var selectMenuOption = new SelectMenuOptionBuilder
                {
                    Label = venue.Name,
                    Description = $"Open for the next {venue.Resolution!.End.UtcDateTime.ToNow()[3..]}",
                    Value = venue.Id
                };
                selectMenuBuilder.AddOption(selectMenuOption);
            }
            componentBuilder.WithSelectMenu(selectMenuBuilder);

            await c.Interaction.RespondAsync(MessageRepository.WhatsOpenMessage.PickRandom(), componentBuilder.Build());
        }

        private async Task HandleVenueSelection(ComponentVeniInteractionContext context)
        {
            var selectedVenueId = context.Interaction.Data.Values.Single();
            var asker = context.Interaction.User.Id;
            var venue = this._venues.FirstOrDefault(v => v.Id == selectedVenueId);

            await context.Interaction.Channel.SendMessageAsync(embed: this._venueRenderer.Render(venue).Build(),
                components: this._venueRenderer.RenderActionComponents(context, venue, asker).Build());
        }
    }
}
