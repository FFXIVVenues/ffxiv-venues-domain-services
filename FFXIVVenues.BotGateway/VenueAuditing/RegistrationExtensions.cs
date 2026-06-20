using FFXIVVenues.BotGateway.Infrastructure.Components;
using FFXIVVenues.BotGateway.VenueAuditing.ComponentHandlers.AuditResponse;
using FFXIVVenues.BotGateway.VenueAuditing.ComponentHandlers;

namespace FFXIVVenues.BotGateway.VenueAuditing;

public static class RegistrationExtensions
{

    public static T AddVenueAuditingHandlers<T>(this T componentBroker) where T : IComponentBroker
    {
        if (componentBroker == null)
            return default;

        componentBroker.Add<ConfirmCorrectHandler>(ConfirmCorrectHandler.Key);
        componentBroker.Add<EditVenueHandler>(EditVenueHandler.Key);
        componentBroker.Add<TemporarilyClosedHandler>(TemporarilyClosedHandler.Key);
        componentBroker.Add<PermanentlyClosedHandler>(PermanentlyClosedHandler.Key);
        
        return componentBroker;
    }
}