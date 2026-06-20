using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils.Broadcasting;
using FFXIVVenues.Veni.VenueRendering;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueAuditing;

public class VenueAuditService(
    IDiscordClient client,
    IVenueRenderer venueRenderer,
    IRepository repository,
    IApiService apiService)
    : IVenueAuditService
{
    public VenueAudit CreateAuditFor(Venue venue, string roundId, ulong requestedIn, ulong requestedBy) =>
        new (venue, roundId, requestedIn, requestedBy, client, venueRenderer, repository);
    
    public VenueAudit CreateAuditFor(Venue venue, VenueAuditRecord record) =>
        new (venue, record, client, venueRenderer, repository);

    public Task<VenueAuditRecord> GetAudit(string auditId) =>
        repository.GetByIdAsync<VenueAuditRecord>(auditId);
    
    public Task<VenueAuditRecord> GetLatestRecordFor(Venue venue) =>
        this.GetLatestRecordFor(venue.Id);

    public async Task<VenueAuditRecord> GetLatestRecordFor(string venueId)
    {
        var query = await repository.GetWhereAsync<VenueAuditRecord>(a => a.VenueId == venueId);
        return query.OrderByDescending(a => a.SentTime).Take(1).ToList().FirstOrDefault();
    }

    public async Task UpdateAuditStatus(Venue venue, ulong actingUserId, VenueAuditStatus status) =>
        await UpdateAuditStatus(await GetLatestRecordFor(venue), venue, actingUserId, status);
    
    public async Task UpdateAuditStatus(VenueAuditRecord audit, ulong actingUserId, VenueAuditStatus status) =>
        await UpdateAuditStatus(audit, await apiService.GetVenueAsync(audit.VenueId), actingUserId, status);
    
    public async Task UpdateAuditStatus(VenueAuditRecord audit, Venue venue, ulong actingUserId, VenueAuditStatus status)
    {
        var actionLanguage = status switch
        {
            VenueAuditStatus.RespondedConfirmed => "confirmed the venue's details",
            VenueAuditStatus.RespondedEdit => "edited the venue details",
            VenueAuditStatus.EditedLater => "edited the venue details",
            VenueAuditStatus.RespondedClose => "temporarily closed the venue",
            VenueAuditStatus.ClosedLater => "temporarily closed the venue",
            VenueAuditStatus.RespondedDelete => "deleted the venue",
            VenueAuditStatus.DeletedLater => "deleted the venue",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, @"Only responsive statuses can be set")
        };

        await this.UpdateMessagesSentToManagers(audit, venue, actingUserId, $"You handled this and {actionLanguage}.",
            $"{MentionUtils.MentionUser(actingUserId)} handled this and {actionLanguage}.");
        
        if (audit.MassAuditId == null && audit.RequestedBy != actingUserId 
                                      && audit.Status is VenueAuditStatus.AwaitingResponse or VenueAuditStatus.Pending)
            await NotifyRequsterOfCompletion(audit, venue, $"{MentionUtils.MentionUser(actingUserId)} {actionLanguage}.");

        audit.Log($"{MentionUtils.MentionUser(actingUserId)} {actionLanguage}.");
        audit.Status = status;
        audit.CompletedBy = actingUserId;
        audit.CompletedAt = DateTime.UtcNow;
        
        await repository.UpsertAsync(audit);
    }

    private async Task UpdateMessagesSentToManagers(VenueAuditRecord audit, Venue venue, ulong actingUserId, 
        string responderMessage, string othersMessage)
    {
        var render = await venueRenderer.ValidateAndRenderAsync(venue);
        foreach (var message in audit.Messages)
        {
            if (message.Status != MessageStatus.Sent) continue;
            var newMessage = responderMessage;
            if (message.UserId != actingUserId)
                newMessage = othersMessage;

            var channel = await client.GetChannelAsync(message.ChannelId);
            (channel as IDMChannel)?.ModifyMessageAsync(message.MessageId, props =>
            {
                props.Components = new ComponentBuilder().Build();
                props.Embeds = new[]
                {
                    render.Build(),
                    new EmbedBuilder().WithDescription(newMessage).Build()
                };
            });
        }
    }
    
    private async Task NotifyRequsterOfCompletion(VenueAuditRecord audit, Venue venue, string message)
    {
        var channel = await client.GetChannelAsync(audit.RequestedIn);
        if (channel == null)
            channel = await client.GetDMChannelAsync(audit.RequestedIn);

        if (channel is SocketTextChannel sTextChannel)
            await sTextChannel.SendMessageAsync(
                $"Hey {MentionUtils.MentionUser(audit.RequestedBy)}! The audit of **{venue.Name}** you requested has been completed. \n{message}");
        if (channel is RestTextChannel rTextChannel)
            await rTextChannel.SendMessageAsync(
                $"Hey {MentionUtils.MentionUser(audit.RequestedBy)}! The audit of **{venue.Name}** you requested has been completed. \n{message}");
        if (channel is RestDMChannel rdmChannel)
            await rdmChannel.SendMessageAsync(
                $"Heyo! The audit of **{venue.Name}** you requested has been completed. \n{message}");
        if (channel is SocketDMChannel sdmChannel)
            await sdmChannel.SendMessageAsync(
                $"Heyo! The audit of **{venue.Name}** you requested has been completed. \n{message}");
    }
}