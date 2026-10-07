using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.BotGateway.Infrastructure.Components;
using FFXIVVenues.BotGateway.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.BotGateway.Utils.Broadcasting;
using FFXIVVenues.BotGateway.VenueAuditing.ComponentHandlers.AuditResponse;
using FFXIVVenues.BotGateway.VenueRendering;
using FFXIVVenues.Veni.VenueAuditing;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.BotGateway.VenueAuditing;

public class VenueAudit(
    Venue venue,
    VenueAuditRecord record,
    IDiscordClient discordClient,
    DtoVenueRenderer venueRenderer,
    IRepository repository)
{
    private const int MIN_DAYS_SINCE_LAST_UPDATE = 25; // 3 weeks

    public VenueAudit(Venue venue, string roundId, ulong requestedIn, ulong requestedBy, IDiscordClient discordClient,
        DtoVenueRenderer venueRenderer, IRepository repository) :
        this(venue,
            record: new()
                { VenueId = venue.Id, MassAuditId = roundId, RequestedIn = requestedIn, RequestedBy = requestedBy }, 
            discordClient, venueRenderer, repository) { }

    public async Task<VenueAuditStatus> AuditAsync(bool doNotSkip = false)
    {
        try
        {
            record.Log($"Venue audit requested by {MentionUtils.MentionUser(record.RequestedBy)}.");
            if (record.MassAuditId != null)
                record.Log($"Venue audit requested as part of mass audit {record.MassAuditId}.");

            if (!doNotSkip && !await this.IsAuditRequired())
            {
                record.Log("Venue audit skipped; it should not be audited.");
                record.Status = VenueAuditStatus.Skipped;
                await repository.UpsertAsync(record);
                return VenueAuditStatus.Skipped;
            }

            record.Log($"Sending venue audit message to {venue.Managers.Count} managers.");

            var venueRenderWithCheck = await venueRenderer.ValidateAndRenderAsync(venue);
            var broadcast = new Broadcast(Guid.NewGuid().ToString(), discordClient)
                .WithMessage(AuditStrings.Prompt)
                .WithEmbed(venueRenderWithCheck)
                .WithComponent(ctx => new ComponentBuilder()
                    .WithSelectMenu(new SelectMenuBuilder()
                        .WithValueHandlers()
                        .WithPlaceholder("Select response")
                        .AddOption(new SelectMenuOptionBuilder()
                            .WithLabel("Confirm Correct")
                            .WithEmote(new Emoji("👍"))
                            .WithDescription("Confirm the details on this venue are correct.")
                            .WithStaticHandler(ConfirmCorrectHandler.Key, record.id))
                        .AddOption(new SelectMenuOptionBuilder()
                            .WithLabel("Edit Venue")
                            .WithEmote(new Emoji("✏️"))
                            .WithDescription("Update the details on this venue.")
                            .WithStaticHandler(EditVenueHandler.Key, record.id))
                        .AddOption(new SelectMenuOptionBuilder()
                            .WithLabel("Temporarily Close")
                            .WithEmote(new Emoji("🔒"))
                            .WithDescription("Put this venue on a hiatus for up to 3 months.")
                            .WithStaticHandler(TemporarilyClosedHandler.Key, record.id))
                        .AddOption(new SelectMenuOptionBuilder()
                            .WithLabel("Permanently Close / Delete")
                            .WithEmote(new Emoji("❌"))
                            .WithDescription("Delete this venue completely.")
                            .WithStaticHandler(PermanentlyClosedHandler.Key, record.id))));
            var broadcastReceipt = await broadcast.SendToAsync(venue.Managers.Select(ulong.Parse).ToArray());

            var successful = broadcastReceipt.BroadcastMessages.Count(m => m.Status == MessageStatus.Sent);
            var totalManagers = venue.Managers.Count;
            foreach (var message in broadcastReceipt.BroadcastMessages)
                record.Log($"Message to {message.UserId}: {message.Log}");
            record.Log($"Sent venue audit message to {successful} of {totalManagers} managers.");
            record.Status = successful > 0 ? VenueAuditStatus.AwaitingResponse : VenueAuditStatus.Failed;
            record.SentTime = DateTime.UtcNow;
            record.Messages = broadcastReceipt.BroadcastMessages;
            await repository.UpsertAsync(record);
            return record.Status;
        }
        catch (Exception e)
        {
            record.Log($"Exception occured during venue audit. {e.Message}");
            record.Status = VenueAuditStatus.Failed;
            await repository.UpsertAsync(record);

            throw;
        }
    }

    public async Task<bool> IsAuditRequired()
    {
        var boundaryDate = DateTime.UtcNow.AddDays(-MIN_DAYS_SINCE_LAST_UPDATE);
        var previousAudits = await repository.GetWhereAsync<VenueAuditRecord>(a => 
            a.VenueId == venue.Id && a.CompletedAt != null);
        var mostRecentCompletedAudit = previousAudits.ToList().MaxBy(a => a.CompletedAt);
        
        var venueLastChangedAt = venue.LastModified;
        var venueCreatedAt = venue.Added;
        var venueLastAuditedAt = mostRecentCompletedAudit?.CompletedAt;

        if (venueCreatedAt > boundaryDate)
        {
            record.Log($"Should not be audited; venue created within the last {MIN_DAYS_SINCE_LAST_UPDATE} days.");
            return false;
        }
        
        if (venueLastChangedAt > boundaryDate)
        {
            record.Log($"Should not be audited; venue updated within the last {MIN_DAYS_SINCE_LAST_UPDATE} days.");
            return false;
        }
        
        if (venueLastAuditedAt > boundaryDate)
        {
            record.Log($"Should not be audited; venue audited within the last {MIN_DAYS_SINCE_LAST_UPDATE} days.");
            return false;
        }

        return true;
    }

}