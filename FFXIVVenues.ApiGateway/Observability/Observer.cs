using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FFXIVVenues.DomainData.Entities.Venues;
using FFXIVVenues.DomainData.Helpers;
using FFXIVVenues.VenueModels.Observability;

namespace FFXIVVenues.ApiGateway.Observability
{
    public class Observer : ObserveRequest
    {
        public string Id { get; set; } = IdHelper.GenerateId(8);

        public Func<ObservableOperation, Venue, Task> ObserverAction { get; set; }

        public Observer(IEnumerable<ObservableOperation> operationCriteria,
            ObservableKey? keyCriteria, string valueCriteria) 
            : base(operationCriteria, keyCriteria, valueCriteria)
        {  }

    }
}