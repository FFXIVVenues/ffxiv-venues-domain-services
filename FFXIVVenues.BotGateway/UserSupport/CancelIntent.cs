using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.UserSupport
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
