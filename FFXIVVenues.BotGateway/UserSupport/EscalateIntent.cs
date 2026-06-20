using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Intent;
using FFXIVVenues.BotGateway.VenueControl.VenueAuthoring;
using FFXIVVenues.BotGateway.Authorisation.Configuration;
using FFXIVVenues.BotGateway.Utils.Broadcasting;

namespace FFXIVVenues.BotGateway.UserSupport
{
    internal class EscalateIntent : IntentHandler
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly NotificationsConfiguration _notificationsConfiguration;

        public EscalateIntent(DiscordSocketClient discordClient, NotificationsConfiguration notificationsConfiguration)
        {
            this._discordClient = discordClient;
            this._notificationsConfiguration = notificationsConfiguration;
        }

        public override async Task Handle(VeniInteractionContext context)
        {
            await context.Interaction.RespondAsync($"Alright! I've messaged the family! They'll contact you soon!");

            // Create broadcast factory
            _ = new Broadcast(Guid.NewGuid().ToString(), this._discordClient)
                .WithMessage($"Heyo, I have {context.Interaction.User.Mention} needing some help. :cry: They said \n> {context.Interaction.Content}")
                .SendToAsync(this._notificationsConfiguration.Help);
        }

    }
}
