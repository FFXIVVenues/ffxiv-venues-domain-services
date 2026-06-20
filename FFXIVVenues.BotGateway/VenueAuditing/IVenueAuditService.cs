using System.Threading.Tasks;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueAuditing;

public interface IVenueAuditService
{
    VenueAudit CreateAuditFor(Venue venue, string roundId, ulong requestedIn, ulong requestedBy);
    VenueAudit CreateAuditFor(Venue venue, VenueAuditRecord record);
    Task<VenueAuditRecord> GetAudit(string auditId);
    Task<VenueAuditRecord> GetLatestRecordFor(Venue venue);
    Task<VenueAuditRecord> GetLatestRecordFor(string venueId);
    Task UpdateAuditStatus(Venue venue, ulong actingUserId, VenueAuditStatus status);
    Task UpdateAuditStatus(VenueAuditRecord audit, ulong actingUserId, VenueAuditStatus status);
    Task UpdateAuditStatus(VenueAuditRecord audit, Venue venue, ulong actingUserId, VenueAuditStatus status);
}