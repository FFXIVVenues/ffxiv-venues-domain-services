using FFXIVVenues.VenueModels;
using System;
using FFXIVVenues.VenueModels.Observability;
using Venue = FFXIVVenues.DomainData.Entities.Venues.Venue;

namespace FFXIVVenues.ApiGateway.Observability
{
    public interface IChangeBroker
    {
        Action Observe(Observer observer, InvocationKind invocationKind);
        void Queue(ObservableOperation operation, Venue venue);
    }
}