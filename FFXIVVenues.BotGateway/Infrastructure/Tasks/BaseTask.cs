using System;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.Infrastructure.Tasks;

public abstract class BaseTask : IEntity
{
    public string id { get; set; } = IdHelper.GenerateId(8);
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    public DateTime? StartedAt { get; set; }
    public DateTime? PausedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    public void SetStarted()
    {
        this.StartedAt ??= DateTime.UtcNow;
        this.PausedAt = null;
        this.Status = TaskStatus.Active;
    }

    public void SetPaused()
    {
        this.Status = TaskStatus.Paused;
        this.PausedAt = DateTime.UtcNow;
    }

    public void SetClosed()
    {
        this.Status = TaskStatus.Closed;
    }

    public void SetCompleted()
    {
        this.CompletedAt = DateTime.UtcNow;
        this.Status = TaskStatus.Complete;
    }

    public void SetCancelled()
    {
        this.CompletedAt = DateTime.UtcNow;
        this.Status = TaskStatus.Cancelled;
    }
    
}
