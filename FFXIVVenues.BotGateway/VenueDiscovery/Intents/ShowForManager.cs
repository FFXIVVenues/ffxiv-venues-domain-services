using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.VenueControl;
using FFXIVVenues.Veni.VenueDiscovery.SessionStates;

namespace FFXIVVenues.Veni.VenueDiscovery.Intents
{
    internal class ShowForManager : IIntentHandler
    {

        private readonly IApiService _apiService;

        public ShowForManager(IApiService apiService)
        {
            this._apiService = apiService;
        }

        // todo: change to stateless handlers (like edit)
        public Task Handle(MessageVeniInteractionContext context)
        {
            var discordIdStr = context.Prediction?.Entities.FirstOrDefault(e => e.Category == "discord-id")?.Text;

            if (string.IsNullOrWhiteSpace(discordIdStr))
                return context.Interaction.Channel.SendMessageAsync("Which manager am I getting venues for? 🤔");

            var discordId = ulong.Parse(discordIdStr);

            return this.Handle(context.ToWrappedInteraction(), discordId);
        }

        public Task Handle(ComponentVeniInteractionContext context) =>
            Task.CompletedTask;

        public Task Handle(SlashCommandVeniInteractionContext context)
        {
            var user = context.Interaction.Data?.Options?.FirstOrDefault(o => o.Name == "user")?.Value as SocketUser;
            return this.Handle(context.ToWrappedInteraction(), user.Id);
        }

        private async Task Handle(VeniInteractionContext c, ulong discordId)
        {
            var venues = await this._apiService.GetAllVenuesAsync(discordId);

            if (venues == null || !venues.Any())
            {
                await c.Interaction.RespondAsync("Couldn't find any venues for that manager. 😔");
                return;
            }

            if (venues.Count() > 25)
                venues = venues.Take(25);
            c.Session.SetItem(SessionKeys.VENUES, venues);
            await c.Session.MoveStateAsync<SelectVenueToShowSessionState>(c);
        }

    }
}
