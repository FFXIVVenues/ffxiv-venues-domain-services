using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueEvents;

[DiscordCommandRestrictToMasterGuild]
[DiscordCommand("stream", "Stream venue events into this channel.", GuildPermission.ManageChannels)]
[DiscordCommandOption("event", "The type of events to stream into the channel.", ApplicationCommandOptionType.String, Required = true)]
[DiscordCommandOptionChoice("event", "Venue Flags", nameof(StreamableEvent.Flags))]
[DiscordCommandOptionChoice("event", "Venue Creations", nameof(StreamableEvent.Created))]
[DiscordCommandOptionChoice("event", "Venue Edits", nameof(StreamableEvent.Edits))]
[DiscordCommandOptionChoice("event", "Venue Deletion", nameof(StreamableEvent.Delete))]
[DiscordCommandOptionChoice("event", "Venue Approvals", nameof(StreamableEvent.Approved))]
public class StreamCommand(IAuthorizer authorizer, IRepository repository) : ICommandHandler
{
    public async Task HandleAsync(SlashCommandVeniInteractionContext context)
    {
        var authorized = authorizer.Authorize(context.Interaction.User.Id, Permission.StreamEvents, null);
        if (!authorized.Authorized)
        {
            await context.Interaction.RespondAsync("Sorry, I can't let you do that. 👀", ephemeral: true);
            return;
        }

        var channelId = context.Interaction.Channel.Id;
        var eventType = context.GetEnumArg<StreamableEvent>("event");
        if (eventType is null)
            return;
        
        var existingQuery = await repository.GetWhereAsync<EventStreamChannel>(
            i => i.ChannelId == channelId && i.EventType == eventType.Value);
        var existing = existingQuery.FirstOrDefault();
        if (existing is not null)
        {
            await repository.DeleteAsync(existing);
            await context.Interaction.RespondAsync($"Okay! I won't stream those events to this channel anymore.");
            return;
        }
        
        await repository.UpsertAsync(new EventStreamChannel(channelId, eventType!.Value));
        await context.Interaction.RespondAsync($"Alright! I'll stream those events to this channel. ♥");
    }
}
