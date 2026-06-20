#pragma warning disable CS9107

using System;
using System.Linq;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Infrastructure.Tasks;
using FFXIVVenues.Veni.VenueEvents;
using FFXIVVenues.Veni.VenueRendering;
using FFXIVVenues.VenueModels;
using FFXIVVenues.VenueService.Client.Events;
using Serilog;
using TaskStatus = FFXIVVenues.Veni.Infrastructure.Tasks.TaskStatus;

namespace FFXIVVenues.Veni.VenueAuditing.MassAuditDelete;

public class MassDeleteService(IRepository repository, IApiService apiService, IDiscordClient discordClient) : BaseTaskService<MassDeleteTask>(repository)
{
    
    public async Task<MassDeleteSummary> GetSummary()
    {
        var task = await this.GetTaskAsync();
        return new()
        {
            id = task.id,
            MassAuditId = task.MassAuditId,
            Status = task.Status,
            RequestedBy = task.RequestedBy,
            StartedAt = task.StartedAt,
            PausedAt = task.PausedAt,
            CompletedAt = task.CompletedAt,
            TotalVenues = task.VenuesToDelete.Count,
            VenuesDeleted = task.VenuesToDelete.Count(u => u.Status == DeleteStatus.Deleted),
            VenuesFailedToDelete = task.VenuesToDelete.Count(u => u.Status == DeleteStatus.Failed),
            VenuesPending = task.VenuesToDelete.Count(u => u.Status == DeleteStatus.Pending),
        };
    }

    
    public override async Task Execute(MassDeleteTask taskContext, CancellationToken cancellationToken)
    {
        try
        {
            if (taskContext.Status == TaskStatus.Pending)
            {
                taskContext.SetStarted();
                await repository.UpsertAsync(taskContext);
            }

            taskContext.Log($"Starting deletion of venues");
            Log.Debug("Mass delete: starting deletion of venues");

            var remainingVenues = taskContext.VenuesToDelete.Where(t => t.Status == DeleteStatus.Pending);
            foreach (var remainingVenue in remainingVenues)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var result = await apiService.DeleteVenueAsync(remainingVenue.VenueId);
                remainingVenue.Status = result.IsSuccessStatusCode ? DeleteStatus.Deleted : DeleteStatus.Failed;
                await repository.UpsertAsync(taskContext);

                if (result.IsSuccessStatusCode)
                {
                    taskContext.Log($"Deleted venue {remainingVenue.VenueId}");
                    Log.Debug("Mass delete: deleted venue {VenueId}", remainingVenue.VenueId);
                }
                else
                {
                    // 404 would be if the venue no longer exists, so we can ignore it
                    taskContext.Log($"Could not delete venue {remainingVenue.VenueId}: {result.StatusCode}");
                    Log.Debug("Mass delete: Could not delete venue {VenueId}: {Error}", remainingVenue.VenueId,
                        result.StatusCode);
                }

                taskContext.Log($"Completed deletion of venues.");
                taskContext.SetCompleted();
                await repository.UpsertAsync(taskContext);
                
                if (result.IsSuccessStatusCode)
                {
                    var venue = await result.Content.ReadFromJsonAsync<Venue>(cancellationToken);
                    new VenueDeletedHandler(repository, discordClient).Handle(
                        new VenueDeletedEvent(remainingVenue.VenueId, venue.Name, 2));
                }

                if (await discordClient.GetChannelAsync(taskContext.RequestedIn) is not IMessageChannel channel)
                    channel = await discordClient.GetDMChannelAsync(taskContext.RequestedIn);
                await channel.SendMessageAsync(
                    $"Hey {MentionUtils.MentionUser(taskContext.RequestedBy)}, I've completed the deletes!");

                Log.Debug("Mass delete: completed deletion of venues");
            }

        }
        catch (Exception e)
        {
            Log.Error(e, "Exception occured in mass delete execution");
        }
    }
}