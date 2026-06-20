using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils.Broadcasting;
using FFXIVVenues.Veni.VenueAuditing.ComponentHandlers;
using FFXIVVenues.Veni.VenueAuditing.ComponentHandlers.AuditResponse;
using FFXIVVenues.Veni.VenueControl;
using FFXIVVenues.Veni.VenueRendering;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueAuditing;

public class VenueAudit
{
    private const int MIN_DAYS_SINCE_LAST_UPDATE = 25; // 3 weeks
    
    private readonly VenueAuditRecord _record;
    private readonly Venue _venue;
    private readonly IDiscordClient _discordClient;
    private readonly IVenueRenderer _venueRenderer;
    private readonly IRepository _repository;

    public VenueAudit(Venue venue, string roundId, ulong requestedIn, ulong requestedBy, IDiscordClient discordClient,
        IVenueRenderer venueRenderer, IRepository repository) :
        this(venue,
            record: new()
                { VenueId = venue.Id, MassAuditId = roundId, RequestedIn = requestedIn, RequestedBy = requestedBy }, 
            discordClient, venueRenderer, repository) { }

    public VenueAudit(Venue venue,
        VenueAuditRecord record,
        IDiscordClient discordClient,
        IVenueRenderer venueRenderer,
        IRepository repository)
    {
        this._venue = venue;
        this._record = record;
        this._discordClient = discordClient;
        this._venueRenderer = venueRenderer;
        this._repository = repository;
    }

    public async Task<VenueAuditStatus> AuditAsync(bool doNotSkip = false)
    {
        try
        {
            this._record.Log($"Venue audit requested by {MentionUtils.MentionUser(this._record.RequestedBy)}.");
            if (this._record.MassAuditId != null)
                this._record.Log($"Venue audit requested as part of mass audit {this._record.MassAuditId}.");

            if (!doNotSkip && !await this.IsAuditRequired())
            {
                this._record.Log("Venue audit skipped; it should not be audited.");
                this._record.Status = VenueAuditStatus.Skipped;
                await this._repository.UpsertAsync(this._record);
                return VenueAuditStatus.Skipped;
            }

            this._record.Log($"Sending venue audit message to {this._venue.Managers.Count} managers.");

            var venueRenderWithCheck = await this._venueRenderer.ValidateAndRenderAsync(this._venue);
            var broadcast = new Broadcast(Guid.NewGuid().ToString(), this._discordClient)
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
                            .WithStaticHandler(ConfirmCorrectHandler.Key, this._record.id))
                        .AddOption(new SelectMenuOptionBuilder()
                            .WithLabel("Edit Venue")
                            .WithEmote(new Emoji("✏️"))
                            .WithDescription("Update the details on this venue.")
                            .WithStaticHandler(EditVenueHandler.Key, this._record.id))
                        .AddOption(new SelectMenuOptionBuilder()
                            .WithLabel("Temporarily Close")
                            .WithEmote(new Emoji("🔒"))
                            .WithDescription("Put this venue on a hiatus for up to 3 months.")
                            .WithStaticHandler(TemporarilyClosedHandler.Key, this._record.id))
                        .AddOption(new SelectMenuOptionBuilder()
                            .WithLabel("Permanently Close / Delete")
                            .WithEmote(new Emoji("❌"))
                            .WithDescription("Delete this venue completely.")
                            .WithStaticHandler(PermanentlyClosedHandler.Key, this._record.id))));
            var broadcastReceipt = await broadcast.SendToAsync(this._venue.Managers.Select(ulong.Parse).ToArray());

            var successful = broadcastReceipt.BroadcastMessages.Count(m => m.Status == MessageStatus.Sent);
            var totalManagers = this._venue.Managers.Count;
            foreach (var message in broadcastReceipt.BroadcastMessages)
                this._record.Log($"Message to {message.UserId}: {message.Log}");
            this._record.Log($"Sent venue audit message to {successful} of {totalManagers} managers.");
            this._record.Status = successful > 0 ? VenueAuditStatus.AwaitingResponse : VenueAuditStatus.Failed;
            this._record.SentTime = DateTime.UtcNow;
            this._record.Messages = broadcastReceipt.BroadcastMessages;
            await this._repository.UpsertAsync(this._record);
            return this._record.Status;
        }
        catch (Exception e)
        {
            this._record.Log($"Exception occured during venue audit. {e.Message}");
            this._record.Status = VenueAuditStatus.Failed;
            await this._repository.UpsertAsync(this._record);

            throw;
        }
    }

    public async Task<bool> IsAuditRequired()
    {
        var boundaryDate = DateTime.UtcNow.AddDays(-MIN_DAYS_SINCE_LAST_UPDATE);
        var previousAudits = await this._repository.GetWhereAsync<VenueAuditRecord>(a => 
            a.VenueId == this._venue.Id && a.CompletedAt != null);
        var mostRecentCompletedAudit = previousAudits.ToList().MaxBy(a => a.CompletedAt);
        
        var venueLastChangedAt = this._venue.LastModified;
        var venueCreatedAt = this._venue.Added;
        var venueLastAuditedAt = mostRecentCompletedAudit?.CompletedAt;

        if (venueCreatedAt > boundaryDate)
        {
            this._record.Log($"Should not be audited; venue created within the last {MIN_DAYS_SINCE_LAST_UPDATE} days.");
            return false;
        }
        
        if (venueLastChangedAt > boundaryDate)
        {
            this._record.Log($"Should not be audited; venue updated within the last {MIN_DAYS_SINCE_LAST_UPDATE} days.");
            return false;
        }
        
        if (venueLastAuditedAt > boundaryDate)
        {
            this._record.Log($"Should not be audited; venue audited within the last {MIN_DAYS_SINCE_LAST_UPDATE} days.");
            return false;
        }

        return true;
    }

}