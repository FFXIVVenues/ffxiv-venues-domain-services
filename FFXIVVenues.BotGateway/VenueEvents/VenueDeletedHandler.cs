using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.BotGateway.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.DomainData.Context;
using FFXIVVenues.VenueService.Client.Events;
using Serilog;

namespace FFXIVVenues.BotGateway.VenueEvents;

public class VenueDeletedHandler(IRepository repository, IDiscordClient client, DomainDataContext db)
{
    public async Task HandleAsync(VenueDeletedEvent @event)
    {
        var streams = await repository.GetWhereAsync<EventStreamChannel>(
            i => i.EventType == StreamableEvent.Created);
        if (!streams.Any()) 
            return;

        var venue = await db.Venues.FindAsync(@event.VenueId);
        if (venue == null) return;
        
        var embed = new EmbedBuilder()
            .WithTitle(venue.Name)
            .WithAuthor("🗑️ Venue Deleted")
            
            .WithColor(Color.Red);
        if (@event.Actor != 0)
            embed.WithDescription("**By** " + @event.Actor switch
            {
                2 => "Mass Audit Delete",
                _ => MentionUtils.MentionUser(@event.Actor)
            });
        
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


