using System;

namespace FFXIVVenues.Veni.VenueAuditing.MassAudit.Models;

public class MassAuditStatusSummary
{
    
    public string id { get; set; } 
    public MassAuditStatus Status { get; set; }
    public ulong RequestedBy { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? PausedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    
    public int TotalVenues { get; set; }
    public int AuditsProcessed { get; set; }
    public int AuditsAnswered { get; set; }
    
    public int VenuesConfirmed { get; set; }
    public int VenuesEdited { get; set; }
    public int VenuesClosed { get; set; }
    public int VenuesDeleted { get; set; }
    
    public int AuditsAwaitingAnswer { get; set; }
    public int AuditsSkipped { get; set; }
    public int AuditsFailed { get; set; }
    public int AuditsInProgress { get; set; }
    
}