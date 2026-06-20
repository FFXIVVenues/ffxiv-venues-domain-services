using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Infrastructure.Tasks;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.Utils.Broadcasting;
using Serilog;
using TaskStatus = FFXIVVenues.Veni.Infrastructure.Tasks.TaskStatus;

namespace FFXIVVenues.Veni.VenueAuditing.MassAuditNotice;

public class MassNoticeService(IRepository repository, IDiscordClient discordClient) 
    : BaseTaskService<MassNoticeTask>(repository)
{

    public async Task<MassNoticeSummary> GetSummary()
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
            TotalUsers = task.TargetUsers.Count,
            NoticesSent = task.TargetUsers.Count(u => u.Status == NoticeStatus.Complete),
            NoticesFailed = task.TargetUsers.Count(u => u.Status == NoticeStatus.Failed),
            NoticesPending = task.TargetUsers.Count(u => u.Status == NoticeStatus.Pending),
        };
    }
    
    public override async Task Execute(MassNoticeTask taskContext, CancellationToken cancellationToken)
    {
        try
        {
            if (taskContext.Status == TaskStatus.Pending)
            {
                taskContext.SetStarted();
                await repository.UpsertAsync(taskContext);
            }

            taskContext.Log($"Starting broadcast of notice \"{taskContext.Message}\"");
            Log.Debug("Mass notice: starting broadcast of notice");

            var remainingUsers = taskContext.TargetUsers.Where(t => t.Status == NoticeStatus.Pending);
            foreach (var user in remainingUsers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    taskContext.Log($"Sending notice to {user.UserId} for venues.");
                    Log.Debug("Mass notice: sending notice to {User}: {Notice}", user.UserId, user.Message);

                    var broadcastReceipt = await new Broadcast(IdHelper.GenerateId(8), discordClient)
                        .WithMessage(user.Message)
                        .SendToAsync(user.UserId);

                    var broadcastMessage = broadcastReceipt.BroadcastMessages.Single();
                    user.Status = broadcastMessage.Status == MessageStatus.Sent 
                        ? NoticeStatus.Complete : NoticeStatus.Failed;

                    taskContext.Log($"Notice to {user.UserId}: {broadcastMessage.Log} ({broadcastMessage.Status})");
                    Log.Debug("Mass notice: notice to {User}: {NoticeResultMessage} ({NoticeResult})", 
                        user.UserId, broadcastMessage.Log, broadcastMessage.Status);
                }
                catch (Exception e)
                {
                    Log.Warning(e, "Mass notice: exception occured while sending notice to {User}", user);
                    taskContext.Log($"Exception while sending notice to {user}. {e.Message}");
                }

                await repository.UpsertAsync(taskContext);
                await Task.Delay(3000, cancellationToken);
            }

            taskContext.Log($"Completed broadcast of notice.");
            taskContext.SetCompleted();
            await repository.UpsertAsync(taskContext);

            if (await discordClient.GetChannelAsync(taskContext.RequestedIn) is not IMessageChannel channel)
                channel = await discordClient.GetDMChannelAsync(taskContext.RequestedIn);
            await channel.SendMessageAsync(
                $"Hey {MentionUtils.MentionUser(taskContext.RequestedBy)}, I've completed sending all notices!");

            Log.Debug("Mass notice: completed broadcast of notice");
        }
        catch (Exception e)
        {
            Log.Error(e, "Exception occured in mass notice execution");
        }
    }
    
}