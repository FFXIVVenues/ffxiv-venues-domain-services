using System;
using System.Collections.Generic;
using FFXIVVenues.Veni.Infrastructure.Tasks;

namespace FFXIVVenues.Veni.VenueAuditing.MassAuditNotice;

public class MassNoticeTask : BaseTask
{
    public required string MassAuditId { get; init; }
    public required ulong RequestedBy { get; init; }
    public required ulong RequestedIn { get; init; }
    public required string Message { get; init; }
    public required List<NoticeTarget> TargetUsers { get; set; } = new();
    public List<SimpleLogRecord> Logs { get; private set; } = new();
    
    public void Log(string message) =>
        this.Logs.Add(new (DateTime.UtcNow, message));
}

public record NoticeTarget(ulong UserId, string Message)
{
    public NoticeStatus Status { get; set; } = NoticeStatus.Pending;

    public override int GetHashCode() =>
        UserId.GetHashCode();
}
