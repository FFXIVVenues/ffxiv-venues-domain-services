using System.Threading.Tasks;
using Discord;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Intent;

namespace FFXIVVenues.BotGateway.UserSupport
{
    internal class HelpIntent : IntentHandler
    {

        public override Task Handle(VeniInteractionContext context) =>
            context.Interaction.RespondAsync(UserSupportStrings.HelpResponseMessage, 
                embed: new EmbedBuilder().WithDescription(UserSupportStrings.HelpResponseEmbed).Build());

    }
}
