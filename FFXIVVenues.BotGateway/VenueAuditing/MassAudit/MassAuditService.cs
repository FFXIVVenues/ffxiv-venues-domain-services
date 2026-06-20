using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils.Broadcasting;
using FFXIVVenues.Veni.VenueAuditing.MassAudit.Exporting;
using FFXIVVenues.Veni.VenueAuditing.MassAudit.Models;
using FFXIVVenues.Veni.VenueAuditing.MassAuditDelete;
using FFXIVVenues.Veni.VenueAuditing.MassAuditNotice;
using FFXIVVenues.VenueModels;
using Serilog;

namespace FFXIVVenues.Veni.VenueAuditing.MassAudit;

internal class MassAuditService(
    IApiService apiService,
    IRepository repository,
    IVenueAuditService venueAuditService,
    IDiscordClient client,
    IMassAuditExporter massAuditExporter,
    MassNoticeService massNoticeService,
    MassDeleteService massDeleteService)
    : IMassAuditService
{
    private bool _pause = false;
    private bool _cancel = false;
    private Task _activeAuditTask;
    private CancellationTokenSource _cancellationTokenSource;

    #region Mass Audit Api
    public async Task<ResumeResult> ResumeAsync(bool activeOnly)
    {
        Log.Debug("Resume requested for mass audit");
        if (this._activeAuditTask is not null && !this._activeAuditTask.IsCompleted)
        {
            Log.Debug("Mass audit already running");
            return ResumeResult.AlreadyRunning;
        }

        IQueryable<MassAuditRecord> activeAudits;
        if (activeOnly)
        {
            Log.Debug("Fetching active mass audits");
            activeAudits = await repository.GetWhereAsync<MassAuditRecord>(a => a.Status == MassAuditStatus.Active);
        }
        else
        {
            Log.Debug("Fetching active and inactive mass audits");
            activeAudits = await repository.GetWhereAsync<MassAuditRecord>(a => 
                a.Status == MassAuditStatus.Active || a.Status == MassAuditStatus.Inactive);
        }
        var audit = activeAudits.OrderByDescending(a => a.StartedAt).ToList().SingleOrDefault();

        if (audit == null)
        {
            Log.Debug("No mass audit to resume");
            return ResumeResult.NothingToResume;
        }

        var status = audit.Status;

        this.StartThread(audit);

        if (status == MassAuditStatus.Active)
        {
            Log.Debug("Active mass audit resumed");
            return ResumeResult.ResumedActive;
        }
        
        Log.Information("Inactive mass audit {AuditId} resumed", audit.id);
        return ResumeResult.ResumedPaused;
    }

    public async Task<StartResult> StartAsync(ulong requestedIn, ulong requestedBy)
    {
        Log.Debug("Start request for mass audit");
        if (this._activeAuditTask is not null && !this._activeAuditTask.IsCompleted)
        {
            Log.Debug("Active mass audit already exists");
            return StartResult.AlreadyRunning;
        }

        Log.Debug("Fetching active and inactive mass audits");
        var activeAudits = await repository.GetWhereAsync<MassAuditRecord>(a => 
            a.Status == MassAuditStatus.Active ||
            a.Status == MassAuditStatus.Inactive);
        var audit = activeAudits.OrderByDescending(a => a.StartedAt).ToList().SingleOrDefault();
        
        if (audit is not null)
            if (audit.Status == MassAuditStatus.Active)
            {
                Log.Debug("Active mass audit already exists (but is not running)");
                return StartResult.HaultedExists;
            }
            else
            {
                Log.Debug("Inactive mass audit already exists");
                return StartResult.PausedExists;
            }

        _ = massNoticeService.CloseAsync();
        
        audit = new MassAuditRecord
        {
            RequestedIn = requestedIn,
            RequestedBy = requestedBy
        };
        
        this.StartThread(audit);
        
        Log.Information("Mass audit {AuditId} started", audit.id);
        return StartResult.Started;
    }

    public async Task<PauseResult> PauseAsync()
    {
        Log.Debug("Pause request for mass audit");
        var pausedViaTask = false;
        if (this._activeAuditTask is not null && !this._activeAuditTask.IsCompleted)
        {
            Log.Debug("Mass audit cancellation token triggered");
            this._pause = true;
            this._cancellationTokenSource.Cancel();
            await this._activeAuditTask;
            pausedViaTask = true;
        }
        
        Log.Debug("Fetching active and inactive mass audits");
        var audit = (await repository.GetWhereAsync<MassAuditRecord>(a => 
                a.Status == MassAuditStatus.Active || a.Status == MassAuditStatus.Inactive))
            .OrderByDescending(a => a.StartedAt).Take(1).ToList().FirstOrDefault();
        
        if (audit == null)
        {
            Log.Debug("No mass audits to pause");
            return PauseResult.NothingToPause;
        }
        
        if (audit.Status == MassAuditStatus.Closed)
        {
            Log.Debug("Mass audit is closed");
            return PauseResult.Closed;
        }
        
        if (audit.Status == MassAuditStatus.Inactive)
        {
            if (!pausedViaTask) {
                Log.Debug("Mass audit already paused");
                return PauseResult.AlreadyPaused;
            }
            Log.Debug("Mass audit paused");
            return PauseResult.Paused;
        }
        
        audit.SetPaused();
        audit.Log("Mass audit paused");
        await repository.UpsertAsync(audit);
        
        Log.Information("Mass audit {AuditId} paused", audit.id);
        return PauseResult.Paused;
    }

    public async Task<CancelResult> CancelAsync()
    {
        Log.Debug("Cancel request for mass audit");
        var cancelledViaTask = false;
        if (this._activeAuditTask is not null && !this._activeAuditTask.IsCompleted)
        {
            Log.Debug("Mass audit cancellation token triggered");
            this._cancel = true;
            this._cancellationTokenSource.Cancel();
            await this._activeAuditTask;
            cancelledViaTask = true;
        }
        
        Log.Debug("Fetching active and inactive mass audits");
        var activeAudits = (await repository.GetWhereAsync<MassAuditRecord>(a => 
                    a.Status == MassAuditStatus.Active || a.Status == MassAuditStatus.Inactive))
            .OrderByDescending(a => a.StartedAt).ToList();

        if (!activeAudits.Any())
        {
            if (cancelledViaTask)
            {
                Log.Debug("Mass audit cancelled");
                return CancelResult.Cancelled;
            }
            Log.Debug("No mass audits to cancel");
            return CancelResult.NothingToCancel;
        }

        foreach (var audit in activeAudits)
        {
            audit.SetCancelled();
            audit.Log("Mass audit cancelled");
            await repository.UpsertAsync(audit);
            Log.Information("Mass audit {AuditId} cancelled", audit.id);
        }

        Log.Information("All active/inactive mass audits cancelled");
        return CancelResult.Cancelled;
    }

    public async Task<MassAuditStatusSummary> GetSummaryAsync()
    {
        Log.Debug("Mass audit summary requested");
        var activeAuditRounds = await repository.QueryAsync<MassAuditRecord>();
        var auditRound = activeAuditRounds.OrderByDescending(a => a.StartedAt).Take(1).ToList().FirstOrDefault();
        if (auditRound == null)
            return null;

        var audits = await repository.GetWhereAsync<VenueAuditRecord>(a => a.MassAuditId == auditRound.id);
        var allVenues = await apiService.GetAllVenuesAsync();

        return massAuditExporter.GetSummaryForMassAudit(auditRound, allVenues, audits.ToList());
    }

    public async Task<MassAuditStatusReport> GetReportAsync()
    {
        Log.Debug("Mass audit report requested");
        var activeAuditRounds = await repository.QueryAsync<MassAuditRecord>();
        var auditRound = activeAuditRounds.OrderByDescending(a => a.StartedAt).Take(1).ToList().FirstOrDefault();
        if (auditRound == null)
            return null;

        var audits = await repository.GetWhereAsync<VenueAuditRecord>(a => a.MassAuditId == auditRound.id);
        var allVenues = await apiService.GetAllVenuesAsync();
        
        return await massAuditExporter.GetExportForMassAuditAsync(auditRound, allVenues, audits.ToList());
    }
    
    public async Task<CloseResult> CloseMassAudit()
    {
        Log.Debug("Close request for mass audit");
        if (this._activeAuditTask is not null && !this._activeAuditTask.IsCompleted)
        {
            Log.Debug("Mass audit is active");
            return CloseResult.StillRunning;
        }
        
        Log.Debug("Fetching most recent mass audits");
        var latestMassAudit = await this.GetTaskAsync();

        if (latestMassAudit is null)
        {
            Log.Debug("No mass audits to close");
            return CloseResult.NothingToClose;
        }
        
        if (latestMassAudit.Status == MassAuditStatus.Active)
        {
            Log.Debug("Mass audit is active");
            return CloseResult.StillRunning;
        }

        if (latestMassAudit.Status == MassAuditStatus.Closed)
        {
            Log.Debug("Mass audit is already closed");
            return CloseResult.AlreadyClosed;
        }

        latestMassAudit.SetClosed();
        latestMassAudit.Log("Mass audit closed");
        await repository.UpsertAsync(latestMassAudit);
        Log.Debug("Mass audit {AuditId} closed", latestMassAudit.id);
        
        return CloseResult.Closed;
    }
    
    public async Task<MassAuditRecord> GetTaskAsync() => 
        (await repository.QueryAsync<MassAuditRecord>())
            .OrderByDescending(a => a.StartedAt)
            .Take(1)
            .ToList()
            .FirstOrDefault();
    #endregion
    
    #region Notices
    public async Task<NoticeResult> StartNoticeAsync(ulong requestedIn, ulong requestedBy, string notice)
    {
        Log.Debug("Notice requested for mass audit");
        if (this._activeAuditTask is not null && !this._activeAuditTask.IsCompleted)
        {
            Log.Debug("Mass audit is active");
            return NoticeResult.MassAuditRunning;
        }
        
        var latestMassAudit = await this.GetTaskAsync();
        if (latestMassAudit == null)
            return NoticeResult.NoMassAudits;

        if (latestMassAudit.Status is MassAuditStatus.Closed)
            return NoticeResult.MassAuditClosed;

        if (latestMassAudit.Status is not MassAuditStatus.Complete)
            return NoticeResult.MassAuditNotComplete;


        var targetUsers = new List<NoticeTarget>();
        var allVenues = await apiService.GetAllVenuesAsync();
        var awaitingAudits = await repository.GetWhereAsync<VenueAuditRecord>(r =>
            r.MassAuditId == latestMassAudit.id && r.Status == VenueAuditStatus.AwaitingResponse);
        var managers = awaitingAudits
            .SelectMany(a => a.Messages.Where(m => m.Status == MessageStatus.Sent).Select(m => m.UserId))
            .Distinct();

        foreach (var manager in managers)
        {
            var auditsForUser = awaitingAudits.Where(a => a.Messages.Any(m => m.UserId == manager)).ToList();
            
            var venueNamesForUser = auditsForUser
                .Select(a => allVenues.FirstOrDefault(v => a.VenueId == v.Id))
                .Where(v => v != null)
                .Select(v => v.Name)
                .ToList();

            if (!venueNamesForUser.Any())
                continue;
            
            var messageForManager = string.Format(notice, ToReadableList(venueNamesForUser));
            targetUsers.Add(new NoticeTarget(manager,messageForManager));
        }
        
        var startResult = await massNoticeService.StartAsync(new ()
        {
            MassAuditId = latestMassAudit.id,
            RequestedBy = requestedBy,
            RequestedIn = requestedIn,
            Message = notice,
            TargetUsers = targetUsers
        });

        return startResult switch
        {
            StartResult.Started => NoticeResult.Started,
            StartResult.HaultedExists => NoticeResult.NoticeHaulted,
            StartResult.AlreadyRunning => NoticeResult.NoticeAlreadyRunning,
            StartResult.PausedExists => NoticeResult.NoticePausedExists,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public Task<PauseResult> PauseNoticeAsync() =>
        // Allow pausing any task even if it's for a previous audit
        massNoticeService.PauseAsync();

    public async Task<ResumeResult> ResumeNoticeAsync()
    {
        var latestMassAudit = await this.GetTaskAsync();
        if (latestMassAudit is null)
            return ResumeResult.NothingToResume;
        
        var task = await massNoticeService.GetTaskAsync();
        if (latestMassAudit.id == task.MassAuditId)
            return await massNoticeService.ResumeAsync(false);

        return ResumeResult.NothingToResume;
    }

    public Task<CancelResult> CancelNoticeAsync() =>
        // Allow cancelling any task even if it's for a previous audit
        massNoticeService.CancelAsync();

    public async Task<MassNoticeTask> GetNoticeTaskAsync()
    {
        var latestMassAudit = await this.GetTaskAsync();
        if (latestMassAudit is null)
            return null;
        
        var task = await massNoticeService.GetTaskAsync();
        if (task.MassAuditId == latestMassAudit.id)
            return task;

        return null;
    }

    public async Task<MassNoticeSummary> GetNoticeSummaryAsync()
    {
        var noticesTask = await this.GetNoticeTaskAsync();
        return noticesTask is not null
            ? await massNoticeService.GetSummary()
            : null;
    }
    #endregion
    
    #region Deletes

    public async Task<List<Venue>> ProposeDelete()
    {
        var latestMassAudit = await this.GetTaskAsync();
        if (latestMassAudit == null)
            return [];

        var unconfirmedAudits =
            (await repository.GetWhereAsync<VenueAuditRecord>(r =>
                r.MassAuditId == latestMassAudit.id
                && (r.Status == VenueAuditStatus.AwaitingResponse
                    || r.Status == VenueAuditStatus.Failed))).ToList();

        var allVenues = await apiService.GetAllVenuesAsync();
        return allVenues.Where(v => 
            unconfirmedAudits.Any(a => a.VenueId == v.Id)).ToList();
    }
    
    public async Task<DeleteResult> StartDeletesAsync(ulong requestedIn, ulong requestedBy)
    {
        Log.Debug("Deletes requested for mass audit");
        if (this._activeAuditTask is not null && !this._activeAuditTask.IsCompleted)
        {
            Log.Debug("Mass audit is active");
            return DeleteResult.MassAuditRunning;
        }
        
        var latestMassAudit = await this.GetTaskAsync();
        if (latestMassAudit == null)
            return DeleteResult.NoMassAudits;

        if (latestMassAudit.Status is MassAuditStatus.Closed)
            return DeleteResult.MassAuditClosed;

        if (latestMassAudit.Status is not MassAuditStatus.Complete)
            return DeleteResult.MassAuditNotComplete;

        var unconfirmedVenues = 
            await repository.GetWhereAsync<VenueAuditRecord>(r =>
                r.MassAuditId == latestMassAudit.id 
                && (r.Status == VenueAuditStatus.AwaitingResponse 
                   || r.Status == VenueAuditStatus.Failed));
        var unconfirmedVenueIds = unconfirmedVenues
            .Select(v => new VenueTarget(v.VenueId));
        
        var startResult = await massDeleteService.StartAsync(new ()
        {
            MassAuditId = latestMassAudit.id,
            RequestedBy = requestedBy,
            RequestedIn = requestedIn,
            VenuesToDelete = unconfirmedVenueIds.ToList()
        });

        return startResult switch
        {
            StartResult.Started => DeleteResult.Started,
            StartResult.HaultedExists => DeleteResult.DeleteHaulted,
            StartResult.AlreadyRunning => DeleteResult.DeleteAlreadyRunning,
            StartResult.PausedExists => DeleteResult.DeletePausedExists,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public Task<PauseResult> PauseDeletesAsync() =>
        // Allow pausing any task even if it's for a previous audit
        massDeleteService.PauseAsync();

    public async Task<ResumeResult> ResumeDeletesAsync()
    {
        var deletesTask = await this.GetDeletesTaskAsync();
        return deletesTask is null 
            ? ResumeResult.NothingToResume 
            : await massDeleteService.ResumeAsync(false);
    }

    public Task<CancelResult> CancelDeletesAsync() =>
        // Allow cancelling any task even if it's for a previous audit
        massDeleteService.CancelAsync();

    public async Task<MassDeleteSummary> GetDeletesSummaryAsync()
    {
        var deletesTask = await this.GetDeletesTaskAsync();
        return deletesTask is not null
            ? await massDeleteService.GetSummary()
            : null;
    }

    public async Task<MassDeleteTask> GetDeletesTaskAsync()
    {
        var latestMassAudit = await this.GetTaskAsync();
        if (latestMassAudit is null)
            return null;
        
        var task = await massDeleteService.GetTaskAsync();
        return task.MassAuditId == latestMassAudit.id ? task : null;
    }
    #endregion
    
    #region Mass Audit Internal Logic
    private void StartThread(MassAuditRecord massAudit)
    {
        Log.Debug("Start mass audit thread");
        this._cancellationTokenSource?.Dispose();
        this._cancellationTokenSource = new CancellationTokenSource();
        this._activeAuditTask?.Dispose();
        this._activeAuditTask = this.ExecuteMassAudit(massAudit, this._cancellationTokenSource.Token);
    }
    
    private async Task ExecuteMassAudit(MassAuditRecord massAudit, CancellationToken cancellationToken)
    {
        try {
            this._pause = false;
            this._cancel = false;
            
            Log.Debug($"Mass audit: started executing");
            massAudit.SetStarted();
            massAudit.Log("Mass audit started/resumed");
            await repository.UpsertAsync(massAudit);
            
            Log.Debug($"Mass audit: fetching all venues");
            var allVenues = await apiService.GetAllVenuesAsync();
            massAudit.TotalVenuesToAudit = allVenues.Count();
            Log.Debug($"Mass audit: fetching all existing audits for mass audit");
            var auditRecordsQuery =
                await repository.GetWhereAsync<VenueAuditRecord>(r => r.MassAuditId == massAudit.id);
            var auditRecords = auditRecordsQuery.ToList();
            foreach (var venue in allVenues)
            {
                if (cancellationToken.IsCancellationRequested && this._pause)
                {
                    massAudit.SetPaused();
                    massAudit.Log("Mass audit paused");
                    await repository.UpsertAsync(massAudit);
                    return;
                }

                if (cancellationToken.IsCancellationRequested && this._cancel)
                {
                    massAudit.SetCancelled();
                    massAudit.Log("Mass audit cancelled");
                    await repository.UpsertAsync(massAudit);
                    return;
                }
                
                massAudit.Log($"Sending audit for {venue.Name}");
                var existingRecord = auditRecords.FirstOrDefault(r => r.VenueId == venue.Id);

                var auditStatus = VenueAuditStatus.Pending;
                try
                {
                    if (existingRecord == null)
                    {
                        Log.Debug("Mass audit: starting audit for {Venue}", venue);
                        auditStatus = await venueAuditService
                            .CreateAuditFor(venue, massAudit.id, massAudit.RequestedIn, massAudit.RequestedBy)
                            .AuditAsync();
                    }
                    else if (existingRecord.Status == VenueAuditStatus.Pending)
                    {
                        Log.Debug("Mass audit: starting audit for {Venue} (picking up pending audit)", venue);
                        auditStatus = await venueAuditService.CreateAuditFor(venue, existingRecord).AuditAsync();
                    }
                    else
                    {
                        Log.Debug("Mass audit: already sent audit for {Venue}", venue);
                        massAudit.Log($"Already sent audit for {venue.Name}");
                        continue;
                    }
                }
                catch (Exception e)
                {
                    Log.Warning(e, "Mass audit: exception occured while auditing {Venue}", venue);
                    massAudit.Log($"Exception while sending audit for {venue.Name}. {e.Message}");
                    await repository.UpsertAsync(massAudit);
                    await Task.Delay(3000, cancellationToken);
                    continue;
                }

                if (auditStatus == VenueAuditStatus.AwaitingResponse)
                {
                    Log.Debug("Mass audit: sent audit for {Venue}", venue);
                    massAudit.Log($"Sent audit for {venue.Name}");
                }
                else if (auditStatus == VenueAuditStatus.Skipped)
                {
                    Log.Debug("Mass audit: skipped audit for {Venue}", venue);
                    massAudit.Log($"Skipped audit for {venue.Name}");
                } 
                else if (auditStatus == VenueAuditStatus.Failed)
                {
                    Log.Debug("Mass audit: failed to send audit for {Venue}", venue);
                    massAudit.Log($"Failed to send audit for {venue.Name}");
                }

                await repository.UpsertAsync(massAudit);

                if (auditStatus != VenueAuditStatus.Skipped)
                    await Task.Delay(3000, cancellationToken);
            }
            
            Log.Debug("Mass audit: audits sent to all venues");
            massAudit.SetCompleted();
            massAudit.Log("Mass audit complete");
            await repository.UpsertAsync(massAudit);

            Log.Debug("Mass audit: sending completion notification");
            if (await client.GetChannelAsync(massAudit.RequestedIn) is not IMessageChannel channel)
                channel = await client.GetDMChannelAsync(massAudit.RequestedIn);
            await channel.SendMessageAsync($"Hey {MentionUtils.MentionUser(massAudit.RequestedBy)}, I've completed sending all audits!");
            
            Log.Debug($"Mass audit: complete");
        } 
        catch (Exception e)
        {
            Log.Error(e, "Exception occured in mass audit execution");
        }
    }

    private static string ToReadableList(IEnumerable<string> strings)
    {
        var list = strings.ToList();
        if (list.Count > 5)
        {
            var remainingCount = list.Count - 5;
            var firstFiveItems = list.Take(5).ToList();
            firstFiveItems.Add($"{remainingCount} others");
            list = firstFiveItems;
        }
        var result = string.Join(", ", list);
        var lastComma = result.LastIndexOf(',');
        if (lastComma != -1)
            result = result.Remove(lastComma, 1).Insert(lastComma, " and");
        return result;
    }
    #endregion

}

public enum NoticeResult
{
    NoMassAudits,
    MassAuditRunning,
    MassAuditClosed,
    MassAuditNotComplete,
    NoticeAlreadyRunning,
    NoticePausedExists,
    NoticeHaulted,
    Started
}

public enum DeleteResult
{
    NoMassAudits,
    MassAuditRunning,
    MassAuditClosed,
    MassAuditNotComplete,
    DeleteAlreadyRunning,
    DeletePausedExists,
    DeleteHaulted,
    Started
}


public enum PauseResult
{
    NothingToPause,
    AlreadyPaused,
    Closed,
    Paused,
}

public enum CancelResult
{
    NothingToCancel,
    Cancelled
}

public enum StartResult
{
    AlreadyRunning,
    HaultedExists,
    PausedExists,
    Started
}

public enum ResumeResult
{
    AlreadyRunning,
    NothingToResume,
    ResumedActive,
    ResumedPaused
}

public enum CloseResult
{
    AlreadyClosed,
    StillRunning,
    NothingToClose,
    Closed
}