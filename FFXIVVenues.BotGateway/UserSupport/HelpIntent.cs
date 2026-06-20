using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;

namespace FFXIVVenues.Veni.UserSupport
{
    internal class HelpIntent : IntentHandler
    {

        public override Task Handle(VeniInteractionContext context) =>
            context.Interaction.RespondAsync(UserSupportStrings.HelpResponseMessage, 
                embed: new EmbedBuilder().WithDescription(UserSupportStrings.HelpResponseEmbed).Build());

    }
}
