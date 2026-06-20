using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.VenueObservations.CreatedWithoutSplash;
using FFXIVVenues.VenueModels.Observability;

namespace FFXIVVenues.Veni.VenueObservations;

public static class RegistrationExtensions
{
    
    public static T AddVenueObservers<T>(this T apiObservationService) where T : IApiObservationService
    {
        if (apiObservationService == null)
            return default;
        
        apiObservationService.Observe<CreatedWithoutSplashObserver>(ObservableOperation.Create);
        
        return apiObservationService;
    }
    
    public static T AddVenueObservationHandlers<T>(this T componentBroker) where T : IComponentBroker
    {
        if (componentBroker == null)
            return default;
        
        componentBroker.Add<VolunteerComponentHandler>(VolunteerComponentHandler.Key);
        
        return componentBroker;
    }
}