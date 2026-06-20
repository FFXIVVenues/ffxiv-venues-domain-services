using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.BotGateway.Infrastructure.Persistence.Abstraction;
using Serilog;

namespace FFXIVVenues.BotGateway.VenueEvents;

public class VenueDeletedHandler(IRepository repository, IDiscordClient client)
{
    public async Task HandleAsync(VenueDeletedEvent @event)
    {
        var streams = await repository.GetWhereAsync<EventStreamChannel>(
            i => i.EventType == StreamableEvent.Created);
        if (!streams.Any()) 
            return;

        var embed = new EmbedBuilder()
            .WithTitle(@event.VenueName)
            .WithAuthor("🗑️ Venue Deleted")
            .WithDescription("**By** " + @event.UserId switch
                {
                    2 => "Mass Audit Delete",
                    _ => MentionUtils.MentionUser(@event.UserId)
                })
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


