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
using FFXIVVenues.VenueService.Client.Events;
using Serilog;

namespace FFXIVVenues.Veni.VenueEvents;

public class VenueApprovedHandler(IRepository repository, IDiscordClient client, IApiService apiService, UiConfiguration uiConfig)
{
    public async Task Handle(VenueApprovedEvent @event)
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


