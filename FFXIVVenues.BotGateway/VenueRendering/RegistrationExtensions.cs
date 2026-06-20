using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.VenueAuditing.ComponentHandlers;
using FFXIVVenues.Veni.VenueRendering.ComponentHandlers;

namespace FFXIVVenues.Veni.VenueRendering;

public static class RegistrationExtensions
{

    public static T AddVenueRenderingHandlers<T>(this T componentBroker) where T : IComponentBroker
    {
        if (componentBroker == null)
            return default;
        
        componentBroker.Add<AuditHandler>(AuditHandler.Key);
        componentBroker.Add<DismissHandler>(DismissHandler.Key);
        componentBroker.Add<GetAuditHandler>(GetAuditHandler.Key);
        componentBroker.Add<GetAuditsHandler>(GetAuditsHandler.Key);
        
        return componentBroker;
    }
}