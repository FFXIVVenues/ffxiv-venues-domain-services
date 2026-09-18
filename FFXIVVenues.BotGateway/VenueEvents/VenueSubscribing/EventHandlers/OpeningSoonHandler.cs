using Discord;
using Discord.Utils;
using FFXIVVenues.BotGateway.AI.Davinci;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.BotGateway.Utils;
using FFXIVVenues.BotGateway.Utils.Broadcasting;
using FFXIVVenues.BotGateway.VenueEvents.VenueFlags;
using FFXIVVenues.BotGateway.VenueEvents.VenueSubscribing.Models;
using FFXIVVenues.BotGateway.VenueRendering;
using FFXIVVenues.DomainData.Context;
using FFXIVVenues.DomainData.Entities.Flags;
using FFXIVVenues.ScheduleService.Client.Events;
using MomentNet.Display;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVVenues.BotGateway.VenueEvents.VenueSubscribing.Openings;

public class OpeningSoonHandler(IDiscordClient discordClient,
                                DomainDataContext db,
                                IApiService apiService,
                                UiConfiguration uiConfig)
{
    public async Task Handle(OpeningSoonEvent @event)
    {
        Log.Information("Received OpeningSoonEvent for venue {VenueId}", @event.VenueId);

        var venue = await apiService.GetVenueAsync(@event.VenueId);
        if (venue is null)
        {
            Log.Debug("Could not find venue {VenueId}", @event.VenueId);
            return;
        }

        var subscriptions = db.Favorites.Where(s => s.VenueId == @event.VenueId);
        if (!subscriptions.Any())
        {
            Log.Debug("No subscriptions found for venue {VenueId}, ", @event.VenueId);
            return;
        }

        var embed = new EmbedBuilder()
            .WithAuthor(SubscriptionStrings.OpeningSoonTitle)
            .WithTitle(venue.Name)
            .WithUrl(uiConfig.BaseUrl + "/venue/" + venue.Id)
            .WithImageUrl(venue.BannerUri?.ToString())
            .WithDescription(
                SubscriptionStrings.OpenXFromNowAtY.Fmt(@event.From.ToDiscordRelative(), @event.From.ToDiscordShortTime()) + "\n"
                + venue.Location.ToString());

        var broadcast = new Broadcast(Guid.NewGuid().ToString(), discordClient).WithEmbed(embed);
        var recipients = subscriptions.Select(s => s.UserId).ToArray();
        var broadcastReceipt = await broadcast.SendToAsync(recipients);
        Log.Information("Broadcast sent to {RecipientCount} users for venue {VenueId} opening soon", recipients.Length, @event.VenueId);

    }
}
