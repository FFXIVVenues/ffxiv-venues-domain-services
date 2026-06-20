using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.VenueAuditing.MassAudit;
using Serilog;

namespace FFXIVVenues.Veni.Infrastructure.Tasks;

public abstract class BaseTaskService<T>(IRepository repository) : ITaskService<T> where T : BaseTask
{
    private string _taskType = typeof(T).Name;
    private CancellationTokenSource _cancellationTokenSource;
    private Task _activeTask;

    public async Task<PauseResult> PauseAsync()
    {
        Log.Debug("Pause request for {Task}", this._taskType);
        var pausedViaTask = false;
        if (this._activeTask is not null && !this._activeTask.IsCompleted)
        {
            Log.Debug("{Task} cancellation token triggered", this._taskType);
            await this._cancellationTokenSource.CancelAsync();
            await this._activeTask;
            pausedViaTask = true;
        }
        
        Log.Debug("Fetching active, pending, and paused {Task} tasks", this._taskType);
        var task = (await repository.GetWhereAsync<T>(a => 
                a.Status == TaskStatus.Active || a.Status == TaskStatus.Pending || a.Status == TaskStatus.Paused))
            .OrderByDescending(a => a.StartedAt).Take(1).ToList().FirstOrDefault();
        
        if (task == null)
        {
            Log.Debug("No {Task} to pause", this._taskType);
            return PauseResult.NothingToPause;
        }
        
        if (task.Status == TaskStatus.Closed)
        {
            Log.Debug("{Task} is closed", this._taskType);
            return PauseResult.Closed;
        }
        
        if (task.Status == TaskStatus.Pending)
        {
            Log.Debug("{Task} is not started", this._taskType);
            return PauseResult.NothingToPause;
        }
        
        if (task.Status == TaskStatus.Paused)
        {
            if (!pausedViaTask) {
                Log.Debug("{Task} already paused", this._taskType);
                return PauseResult.AlreadyPaused;
            }
            Log.Debug("{Task} paused", this._taskType);
            return PauseResult.Paused;
        }
        
        task.SetPaused();
        await repository.UpsertAsync(task);
        
        Log.Information("{Task} {TaskId} paused", this._taskType, task.id);
        return PauseResult.Paused;
    }
    
    public async Task<CancelResult> CancelAsync()
    {
        Log.Debug("Cancel request for {Task}", this._taskType);
        var cancelledViaTask = false;
        if (this._activeTask is not null && !this._activeTask.IsCompleted)
        {
            Log.Debug("{Task} cancellation token triggered", this._taskType);
            await this._cancellationTokenSource.CancelAsync();
            await this._activeTask;
            cancelledViaTask = true;
        }
        
        Log.Debug("Fetching active, pending and paused {Task} tasks", this._taskType);
        var activeTasks = (await repository.GetWhereAsync<T>(a =>
                a.Status == TaskStatus.Active || a.Status == TaskStatus.Pending || a.Status == TaskStatus.Paused))
            .OrderByDescending(a => a.StartedAt)
            .ToList();

        if (!activeTasks.Any())
        {
            if (cancelledViaTask)
            {
                Log.Debug("{Task} cancelled", this._taskType);
                return CancelResult.Cancelled;
            }
            Log.Debug("No {Task} to cancel", this._taskType);
            return CancelResult.NothingToCancel;
        }

        foreach (var task in activeTasks)
        {
            task.SetCancelled();
            await repository.UpsertAsync(task);
            Log.Information("{Task} {TaskId} cancelled", this._taskType, task.id);
        }

        Log.Information("All incomplete {Task} tasks cancelled", this._taskType);
        return CancelResult.Cancelled;
    }
    
    public async Task<CloseResult> CloseAsync()
    {
        Log.Debug("Close request for {Task}", this._taskType);
        if (this._activeTask is not null && !this._activeTask.IsCompleted)
        {
            Log.Debug("{Task} is running", this._taskType);
            return CloseResult.StillRunning;
        }
        
        Log.Debug("Fetching most recent {Task}", this._taskType);
        var task = (await repository.QueryAsync<T>())
            .OrderByDescending(a => a.StartedAt).Take(1).ToList().FirstOrDefault();

        if (task is null)
        {
            Log.Debug("No {Task} to close", this._taskType);
            return CloseResult.NothingToClose;
        }
        
        if (task.Status == TaskStatus.Active)
        {
            Log.Debug("{Task} is active", this._taskType);
            return CloseResult.StillRunning;
        }

        if (task.Status == TaskStatus.Closed)
        {
            Log.Debug("{Task} is already closed", this._taskType);
            return CloseResult.AlreadyClosed;
        }

        task.SetClosed();
        await repository.UpsertAsync(task);
        Log.Debug("{Task} {TaskId} closed", this._taskType, task.id);
        
        return CloseResult.Closed;
    }

    public async Task<T> GetTaskAsync() => 
        (await repository.QueryAsync<T>())
            .OrderByDescending(a => a.StartedAt)
            .Take(1)
            .ToList()
            .FirstOrDefault();

    public async Task<ResumeResult> ResumeAsync(bool activeOnly)
    {
        Log.Debug("Resume requested for {Task}", this._taskType);
        if (this._activeTask is not null && !this._activeTask.IsCompleted)
        {
            Log.Debug("{Tak} already running", this._taskType);
            return ResumeResult.AlreadyRunning;
        }

        IQueryable<T> activeAudits;
        if (activeOnly)
        {
            Log.Debug("Fetching active {Task}", this._taskType);
            activeAudits = await repository.GetWhereAsync<T>(a => a.Status == TaskStatus.Active);
        }
        else
        {
            Log.Debug("Fetching active, pending and paused {Task} tasks", this._taskType);
            activeAudits = await repository.GetWhereAsync<T>(a => 
                a.Status == TaskStatus.Active || a.Status == TaskStatus.Pending || a.Status == TaskStatus.Paused);
        }
        var task = activeAudits.OrderByDescending(a => a.StartedAt).ToList().SingleOrDefault();

        if (task == null)
        {
            Log.Debug("No {Task} to resume", this._taskType);
            return ResumeResult.NothingToResume;
        }

        var status = task.Status;

        this.StartThread(task);

        if (status == TaskStatus.Active)
        {
            Log.Debug("Active {Task} {TaskId} resumed", this._taskType, task.id);
            return ResumeResult.ResumedActive;
        }
        
        Log.Information("Pending {Task} {TaskId} resumed", this._taskType, task.id);
        return ResumeResult.ResumedPaused;
    }
    
    public async Task<StartResult> StartAsync(T taskObject)
    {
        Log.Debug("Start request for {Task}", this._taskType);
        if (this._activeTask is not null && !this._activeTask.IsCompleted)
        {
            Log.Debug("Active {Task} already exists", this._taskType);
            return StartResult.AlreadyRunning;
        }

        Log.Debug("Fetching active and paused {Tasks} tasks", this._taskType);
        var activeAudits = await repository.GetWhereAsync<T>(a => 
            a.Status == TaskStatus.Active ||
            a.Status == TaskStatus.Paused);
        var existingTask = activeAudits.OrderByDescending(a => a.StartedAt).ToList().SingleOrDefault();
        
        if (existingTask is not null)
            if (existingTask.Status == TaskStatus.Paused)
            {
                Log.Debug("Paused {Task} already exists", this._taskType);
                return StartResult.PausedExists;
            }
            else
            {
                Log.Debug("Active {Task} already exists (but is not running)", this._taskType);
                return StartResult.HaultedExists;
            }
        
        this.StartThread(taskObject);
        
        Log.Information("{Task} {TaskId} started", this._taskType, taskObject.id);
        return StartResult.Started;
    }
    
    private void StartThread(T task)
    {
        Log.Debug("Starting {Task} thread", this._taskType);
        this._cancellationTokenSource?.Dispose();
        this._cancellationTokenSource = new CancellationTokenSource();
        this._activeTask?.Dispose();
        this._activeTask = this.Execute(task, this._cancellationTokenSource.Token);
    }

    public abstract Task Execute(T taskContext, CancellationToken cancellationToken);

}