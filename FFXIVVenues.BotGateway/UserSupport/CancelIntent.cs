using System.Threading.Tasks;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Intent;
using FFXIVVenues.BotGateway.Utils;

namespace FFXIVVenues.BotGateway.UserSupport
{
    internal class CancelIntent : IntentHandler
    {

        public override Task Handle(VeniInteractionContext context)
        {
            if (context.Session.StateStack == null)
                return context.Interaction.RespondAsync(UserSupportStrings.NothingToCancel);

            _ = context.Session.ClearStateAsync(context);
            return context.Interaction.RespondAsync(UserSupportStrings.Cancelled);
        }

    }
}
