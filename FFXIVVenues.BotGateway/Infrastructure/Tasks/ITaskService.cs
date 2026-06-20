using System.Threading.Tasks;
using FFXIVVenues.Veni.VenueAuditing.MassAudit;

namespace FFXIVVenues.Veni.Infrastructure.Tasks;

public interface ITaskService<T>
{
    Task<ResumeResult> ResumeAsync(bool activeOnly = true);
    Task<StartResult> StartAsync(T taskObject);
    Task<PauseResult> PauseAsync();
    Task<CancelResult> CancelAsync();
    Task<CloseResult> CloseAsync();
    Task<T> GetTaskAsync();
}