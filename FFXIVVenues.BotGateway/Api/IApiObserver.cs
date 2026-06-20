using System.Threading.Tasks;
using FFXIVVenues.VenueModels.Observability;

namespace FFXIVVenues.BotGateway.Api;

public interface IApiObserver
{
    Task Handle(Observation observation);
}