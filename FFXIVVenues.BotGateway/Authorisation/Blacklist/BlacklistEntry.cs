using FFXIVVenues.BotGateway.Infrastructure.Persistence.Abstraction;

namespace FFXIVVenues.BotGateway.Authorisation.Blacklist;

internal class BlacklistEntry : IEntity
{
    public string id { get; set; }
    public string Reason  { get; set; }
}