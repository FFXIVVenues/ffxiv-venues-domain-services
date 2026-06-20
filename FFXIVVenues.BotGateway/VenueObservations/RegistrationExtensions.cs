using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Infrastructure.Components;
using FFXIVVenues.BotGateway.VenueObservations.CreatedWithoutSplash;
using FFXIVVenues.VenueModels.Observability;

namespace FFXIVVenues.BotGateway.VenueObservations;

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