using System.Threading.Tasks;
using FFXIVVenues.BotGateway.Infrastructure.Context;

namespace FFXIVVenues.BotGateway.Infrastructure.Intent
{
    internal class NoneIntent : IntentHandler
    {
        public override async Task Handle(VeniInteractionContext context) =>
            await context.Interaction.Channel.SendMessageAsync("X.X");
    }
}
