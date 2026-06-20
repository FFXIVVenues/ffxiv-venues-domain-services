using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.BotGateway.VenueRendering;
using Serilog;

namespace FFXIVVenues.BotGateway.VenueEvents;

public class VenueApprovedHandler(IRepository repository, IDiscordClient client, IApiService apiService, UiConfiguration uiConfig)
{
    public async Task HandleAsync(VenueApprovedEvent @event)
    {
        var streams = await repository.GetWhereAsync<EventStreamChannel>(
            i => i.EventType == StreamableEvent.Created);
        if (!streams.Any()) 
            return;
        
        var venue = await apiService.GetVenueAsync(@event.VenueId);
        if (venue == null) return;
        var embed = new EmbedBuilder()
            .WithTitle(venue.Name)
            .WithUrl(uiConfig.BaseUrl + "/venue/" + venue.Id)
            .WithAuthor("✅ Venue Approved")
            .WithDescription("**By** " + MentionUtils.MentionUser(@event.UserId))
            .WithColor(Color.Green);
        
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


