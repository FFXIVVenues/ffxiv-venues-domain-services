using System.Collections.Generic;
using System.Linq;
using FFXIVVenues.DomainData.Entities.Venues;
using FFXIVVenues.VenueModels.Observability;

namespace FFXIVVenues.ApiGateway.Observability;

public class ObserveRequest(
    IEnumerable<ObservableOperation> operationCriteria,
    ObservableKey? keyCriteria,
    string valueCriteria)
{
    public IEnumerable<ObservableOperation> OperationCriteria { get; set; } = operationCriteria;
    public ObservableKey? KeyCriteria { get; set; } = keyCriteria;
    public string ValueCriteria { get; set; } = valueCriteria;

    public bool Matches(ObservableOperation operation, Venue venue)
    {
        if (venue == null) return false;
        if (!OperationCriteria.Contains(operation)) return false;
        return KeyCriteria switch
        {
            ObservableKey.Id => venue.Id == ValueCriteria,
            ObservableKey.DataCenter => venue.Location.DataCenter == ValueCriteria,
            ObservableKey.World => venue.Location.World == ValueCriteria,
            ObservableKey.Manager => venue.Managers.Contains(ValueCriteria),
            _ => true,
        };
    }

};