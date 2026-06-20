using System.Threading.Tasks;
using FFXIVVenues.BotGateway.VenueAuditing.MassAudit;

namespace FFXIVVenues.BotGateway.Infrastructure.Tasks;

public interface ITaskService<T>
{
    Task<ResumeResult> ResumeAsync(bool activeOnly = true);
    Task<StartResult> StartAsync(T taskObject);
    Task<PauseResult> PauseAsync();
    Task<CancelResult> CancelAsync();
    Task<CloseResult> CloseAsync();
    Task<T> GetTaskAsync();
}