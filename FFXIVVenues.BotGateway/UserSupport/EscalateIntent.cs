using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using FFXIVVenues.Veni.Authorisation.Configuration;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.Utils.Broadcasting;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring;

namespace FFXIVVenues.Veni.UserSupport
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
