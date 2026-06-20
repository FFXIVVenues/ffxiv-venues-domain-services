using System.Threading.Tasks;
using FFXIVVenues.VenueModels.Observability;

namespace FFXIVVenues.Veni.Api;

public interface IApiObserver
{
    Task Handle(Observation observation);
}