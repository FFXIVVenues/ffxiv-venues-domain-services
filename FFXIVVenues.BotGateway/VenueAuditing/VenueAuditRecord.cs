using System;
using System.Collections.Generic;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils.Broadcasting;

namespace FFXIVVenues.Veni.VenueAuditing;

[Serializable]
public class VenueAuditRecord : IEntity
{
    public string id { get; init; }
    public string VenueId { get; init; }
    public string MassAuditId { get; init; }
    public VenueAuditStatus Status { get; set; }
    public DateTime SentTime { get; set; }
    public ulong RequestedIn { get; set; }
    public ulong RequestedBy { get; set; }
    public ulong CompletedBy { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<BroadcastMessageReceipt> Messages { get; set; } = new();
    public List<SimpleLogRecord> Logs { get; set; } = new();

    public void Log(string message) =>
        this.Logs.Add(new SimpleLogRecord(DateTime.UtcNow, message));

    public VenueAuditRecord() =>
        this.id = Guid.NewGuid().ToString().Replace("-", "").Substring(8, 8);
    
}
