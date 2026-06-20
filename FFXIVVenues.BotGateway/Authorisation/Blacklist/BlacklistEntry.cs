using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;

namespace FFXIVVenues.Veni.Authorisation.Blacklist;

internal class BlacklistEntry : IEntity
{
    public string id { get; set; }
    public string Reason  { get; set; }
}