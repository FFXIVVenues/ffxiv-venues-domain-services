using System;
using System.Collections.Generic;
using FFXIVVenues.Veni.Infrastructure.Tasks;

namespace FFXIVVenues.Veni.VenueAuditing.MassAuditDelete;

public class MassDeleteTask : BaseTask
{
    public required string MassAuditId { get; init; }
    public required ulong RequestedBy { get; init; }
    public required ulong RequestedIn { get; init; }
    public required List<VenueTarget> VenuesToDelete { get; init; }
    public List<SimpleLogRecord> Logs { get; private set; } = new();
    
    public void Log(string message) =>
        this.Logs.Add(new (DateTime.UtcNow, message));

}

public record VenueTarget(string VenueId)
{
    public DeleteStatus Status { get; set; } = DeleteStatus.Pending;
}

public enum DeleteStatus
{
    Pending,
    Deleted,
    Failed
}