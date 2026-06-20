using System;
using FFXIVVenues.Veni.Infrastructure.Tasks;

namespace FFXIVVenues.Veni.VenueAuditing.MassAuditDelete;

public class MassDeleteSummary
{
    
    public string id { get; set; }
    public string MassAuditId { get; set; }
    public TaskStatus Status { get; set; }
    public ulong RequestedBy { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? PausedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    public int TotalVenues { get; set; }
    public int VenuesDeleted { get; set; }
    public int VenuesFailedToDelete { get; set; }
    public int VenuesPending { get; set; }
    
}