using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.FlagService.Client.Events;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueRendering;
using Serilog;

namespace FFXIVVenues.Veni.VenueEvents;

public class VenueFlaggedHandler(IRepository repository, IDiscordClient client, IApiService apiService, UiConfiguration uiConfig)
{
    public async Task Handle(VenueFlaggedEvent @event)
    {
        var streams = await repository.GetWhereAsync<EventStreamChannel>(
            i => i.EventType == StreamableEvent.Flags);
        if (!streams.Any()) 
            return;
        
        var venue = await apiService.GetVenueAsync(@event.VenueId);
        if (venue == null) return;
        var embed = new EmbedBuilder()
            .WithTitle(venue.Name)
            .WithAuthor("Venue Flagged")
            .WithUrl(uiConfig.BaseUrl + "/venue/" + venue.Id)
            .WithDescription(
                $"""
                **Category: **{@event.Category}
                **Region: **{FfxivWorlds.GetRegionForDataCenter(venue.Location.DataCenter)}
                {(@event.Description is not null ? "**Description: **" + @event.Description : "")}
                """)
            .WithFooter(@event.From)
            .WithColor(Color.Red);
        
        foreach (var stream in streams)
        {
            var channel = await client.GetChannelAsync(stream.ChannelId);
            if (channel is not SocketTextChannel socketTextChannel)
            {
                Log.Debug("Channel {ChannelId} does not exist or is not a text channel, removing", stream.ChannelId);
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
}


