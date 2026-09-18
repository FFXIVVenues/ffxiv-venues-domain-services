using Discord;
using Discord.WebSocket;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Authorisation;
using FFXIVVenues.BotGateway.Infrastructure.Components;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.BotGateway.Utils;
using FFXIVVenues.BotGateway.Utils.Broadcasting;
using FFXIVVenues.BotGateway.VenueAuditing;
using FFXIVVenues.BotGateway.VenueAuditing.ComponentHandlers.AuditResponse;
using FFXIVVenues.BotGateway.VenueEvents.VenueFlags.Events;
using FFXIVVenues.FlagService.Client;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.BotGateway.VenueEvents.VenueFlags.Responses;

public class ForwardFlagHandler(IVenueFlagRenderer flagRenderer, IDiscordClient discordClient, IRepository repository, IApiService apiService, IAuthorizer authorizer) : IComponentHandler
{
    public static string Key => "FLAG_RESPONSE_FORWARD";

    public async Task HandleAsync(ComponentVeniInteractionContext context, string[] args)
    {
        var flagId = args[0];
        var userId = context.Interaction.User.Id;

        var flag = await repository.GetByIdAsync<VenueFlagDistribution>(flagId);
        var venue = await apiService.GetVenueAsync(flag.Flag.VenueId);

        if (!authorizer.Authorize(context.Interaction.User.Id, Permission.RespondToFlags, venue).Authorized)
        {
            await context.Interaction.FollowupAsync(FlagStrings.NoPermission, flags: MessageFlags.Ephemeral);
            return;
        }

        var flagEmbed = flagRenderer.RenderFlag(venue, flag.Flag);

        var broadcast = new Broadcast(Guid.NewGuid().ToString(), discordClient)
               .WithMessage(FlagStrings.FlagReceived)
               .WithEmbed(flagEmbed)
               .WithComponent(ctx => new ComponentBuilder()
                   .WithSelectMenu(new SelectMenuBuilder()
                       .WithValueHandlers()
                       .WithPlaceholder(FlagStrings.SelectResponse)
                       .AddOption(new SelectMenuOptionBuilder()
                           .WithLabel(FlagStrings.DismissFlag)
                           .WithEmote(new Emoji("❎"))
                           .WithDescription(FlagStrings.DismissFlagDescription)
                           .WithStaticHandler(DismissFlagHandler.Key, flag.id))
                       .AddOption(new SelectMenuOptionBuilder()
                           .WithLabel(FlagStrings.EditVenue)
                           .WithEmote(new Emoji("✏️"))
                           .WithDescription(FlagStrings.EditVenueDescription)
                           .WithStaticHandler(EditVenueFlagHandler.Key, flag.id))
                       .AddOption(new SelectMenuOptionBuilder()
                           .WithLabel(FlagStrings.TemporarilyCloseVenue)
                           .WithEmote(new Emoji("🔒"))
                           .WithDescription(FlagStrings.TemporarilyCloseVenueDescription)
                           .WithStaticHandler(TemporarilyCloseVenueFlagHandler.Key, flag.id))
                       .AddOption(new SelectMenuOptionBuilder()
                            .WithLabel(FlagStrings.PermanentlyClose)
                            .WithEmote(new Emoji("❌"))
                            .WithDescription(FlagStrings.PermanentlyCloseDescription)
                            .WithStaticHandler(PermanentlyCloseVenueFlagHandler.Key, flag.id))
                       ));
        var broadcastReceipt = await broadcast.SendToAsync(venue.Managers.Select(ulong.Parse).ToArray());
        var successful = broadcastReceipt.BroadcastMessages.Where(m => m.Status == MessageStatus.Sent).Select(b => b.UserId);

        var flagOptionsComponent = flagRenderer.RenderFlagOptions(flag.Flag, RenderOptions.ResolveDismiss).Build();

        if (!successful.Any())
        {
            await context.Interaction.FollowupAsync(FlagStrings.CouldNotForward, flags: MessageFlags.Ephemeral);
            return;
        }

        var flagForwardedMessage = FlagStrings.ForwardedFlagTo.Fmt(string.Join(",", successful.Select(MentionUtils.MentionUser)));
        await context.Interaction.FollowupAsync(flagForwardedMessage, flags: MessageFlags.Ephemeral);
        foreach (var message in flag.Messages)
        {
            var channel = await discordClient.GetChannelAsync(message.ChannelId);
            if (channel is not SocketTextChannel socketTextChannel)
            {
                Log.Debug("Could not update flag distribution message, channel {ChannelId} does not exist or is not a text channel, skipping", message.ChannelId);
                continue;
            }

            await socketTextChannel.ModifyMessageAsync(message.MessageId, props =>
            {
                props.Embeds = new[]
                {
                    flagEmbed.Build(),
                    new EmbedBuilder().WithDescription(FlagStrings.UserForwardedFlag.Fmt(MentionUtils.MentionUser(userId))).WithCurrentTimestamp().Build(),
                };
                props.Components = flagOptionsComponent;
            });
        }

        flag.Messages.AddRange(broadcastReceipt.BroadcastMessages.Select(m => new FlagDistributionMessage(m.ChannelId, m.MessageId)));
        await repository.UpsertAsync(flag);
    }
}