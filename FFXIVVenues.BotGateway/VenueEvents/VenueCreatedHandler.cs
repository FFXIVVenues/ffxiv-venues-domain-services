using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.VenueService.Client.Events;
using FFXIVVenues.BotGateway.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.VenueApproval;
using FFXIVVenues.BotGateway.VenueRendering;
using FFXIVVenues.DomainData.Context;
using Serilog;

namespace FFXIVVenues.BotGateway.VenueEvents;

public class VenueCreatedHandler(IRepository repository, IDiscordClient client, DomainDataContext db, VenueApprovalService approvalService, UiConfiguration uiConfig)
{
    public async Task Handle(VenueCreatedEvent @event)
    {
        var venue = await db.Venues.FindAsync(@event.VenueId);
        if (venue == null) return;
        
        var streams = await repository.GetWhereAsync<EventStreamChannel>(
            i => i.EventType == StreamableEvent.Created);
        if (streams.Any())
        {
            var embed = new EmbedBuilder()
                .WithTitle(venue.Name)
                .WithAuthor("🆕 Venue Created")
                .WithUrl(uiConfig.BaseUrl + "/venue/" + venue.Id)
                .WithColor(Color.Green);
            if (@event.Actor != 0)
                embed.WithDescription("**By** " + MentionUtils.MentionUser(@event.Actor));

            foreach (var stream in streams)
            {
                var channel = await client.GetChannelAsync(stream.ChannelId);
                if (channel is not SocketTextChannel socketTextChannel)
                {
                    Log.Debug("Channel {ChannelId} does not exist or is not a text channel, removing",
                        stream.ChannelId);
                    await repository.DeleteAsync(stream);
                    continue;
                }

                try
                {
                    await socketTextChannel.SendMessageAsync(embed: embed.Build());
                }
                catch (Exception e)
                {
                    Log.Error(e, "Could not stream event to channel {ChannelId}", stream.ChannelId);
                }
            }
        }

        if (venue.Approved)
            _ = new VenueApprovedHandler(repository, client, db, uiConfig)
                .HandleAsync(new VenueApprovedEvent(@event.VenueId, @event.Actor));
        else
            await approvalService.SendForApprovalAsync(venue);
    }
}


