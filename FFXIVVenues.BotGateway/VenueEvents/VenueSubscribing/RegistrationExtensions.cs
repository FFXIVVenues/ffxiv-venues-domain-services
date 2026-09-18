using FFXIVVenues.BotGateway.Infrastructure.Components;
using FFXIVVenues.BotGateway.VenueEvents.VenueFlags.Responses;
using FFXIVVenues.BotGateway.VenueEvents.VenueSubscribing.Handlers;

namespace FFXIVVenues.BotGateway.VenueEvents.VenueSubscribing;

public static class RegistrationExtensions
{

    public static T AddVenueSubscriptionHandlers<T>(this T componentBroker) where T : IComponentBroker
    {
        if (componentBroker == null)
            return default;

        componentBroker.Add<SubscribeHandler>(SubscribeHandler.Key);
        componentBroker.Add<UnsubscribeHandler>(UnsubscribeHandler.Key);

        return componentBroker;
    }
}