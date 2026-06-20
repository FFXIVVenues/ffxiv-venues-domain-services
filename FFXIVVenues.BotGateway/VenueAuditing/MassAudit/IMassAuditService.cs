using System.Collections.Generic;
using System.Threading.Tasks;
using FFXIVVenues.BotGateway.VenueAuditing.MassAudit.Models;
using FFXIVVenues.BotGateway.VenueAuditing.MassAuditDelete;
using FFXIVVenues.BotGateway.VenueAuditing.MassAuditNotice;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.BotGateway.VenueAuditing.MassAudit;

public interface IMassAuditService
{
    
    Task<ResumeResult> ResumeAsync(bool activeOnly = true);
    Task<StartResult> StartAsync(ulong requestedIn, ulong requestedBy);
    Task<PauseResult> PauseAsync();
    Task<CancelResult> CancelAsync();
    Task<MassAuditStatusSummary> GetSummaryAsync();
    Task<MassAuditStatusReport> GetReportAsync();
    Task<CloseResult> CloseMassAudit();
    
    Task<NoticeResult> StartNoticeAsync(ulong requestedIn, ulong requestedBy, string message);
    Task<PauseResult> PauseNoticeAsync();
    Task<ResumeResult> ResumeNoticeAsync();
    Task<CancelResult> CancelNoticeAsync();
    Task<MassNoticeSummary> GetNoticeSummaryAsync();
    Task<MassNoticeTask> GetNoticeTaskAsync();

    Task<List<Venue>> ProposeDelete();
    Task<DeleteResult> StartDeletesAsync(ulong requestedIn, ulong requestedBy);
    Task<PauseResult> PauseDeletesAsync();
    Task<ResumeResult> ResumeDeletesAsync();
    Task<CancelResult> CancelDeletesAsync();
    Task<MassDeleteSummary> GetDeletesSummaryAsync();
    Task<MassDeleteTask> GetDeletesTaskAsync();
    
}