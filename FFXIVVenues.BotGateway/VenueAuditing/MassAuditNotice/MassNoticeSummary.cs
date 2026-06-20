using System;
using FFXIVVenues.Veni.Infrastructure.Tasks;

namespace FFXIVVenues.Veni.VenueAuditing.MassAuditNotice;

public class MassNoticeSummary
{
    
    public string id { get; set; }
    public string MassAuditId { get; set; }
    public TaskStatus Status { get; set; }
    public ulong RequestedBy { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? PausedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    public int TotalUsers { get; set; }
    public int NoticesSent { get; set; }
    public int NoticesFailed { get; set; }
    public int NoticesPending { get; set; }
    
}