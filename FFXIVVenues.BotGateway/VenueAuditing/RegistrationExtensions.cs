using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.VenueAuditing.ComponentHandlers;
using FFXIVVenues.Veni.VenueAuditing.ComponentHandlers.AuditResponse;

namespace FFXIVVenues.Veni.VenueAuditing;

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