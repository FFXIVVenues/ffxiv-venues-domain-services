using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;

namespace FFXIVVenues.Veni.Infrastructure.Intent
{
    internal class NoneIntent : IntentHandler
    {
        public override async Task Handle(VeniInteractionContext context) =>
            await context.Interaction.Channel.SendMessageAsync("X.X");
    }
}
