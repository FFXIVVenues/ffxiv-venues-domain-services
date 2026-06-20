using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Authorisation;
using FFXIVVenues.BotGateway.Infrastructure.Components;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.BotGateway.Utils.Broadcasting;
using FFXIVVenues.BotGateway.VenueRendering;
using FFXIVVenues.BotGateway.VenueAuditing;
using OfficeOpenXml.ConditionalFormatting;

namespace FFXIVVenues.BotGateway.VenueObservations.CreatedWithoutSplash;

public class VolunteerComponentHandler(
    IRepository repository,
    IApiService apiService,
    IDiscordClient client,
    IVenueRenderer venueRenderer,
    IAuthorizer authorizer)
    : IComponentHandler
{
    public static string Key => "VENUE-OBS__WITHOUT-SPLASH__CLAIM";

    public async Task HandleAsync(ComponentVeniInteractionContext context, string[] args)
    {
        var broadcastId = args[0];
        var venueId = args[1];
        var venue = await apiService.GetVenueAsync(venueId);
        
        var canPhotograph = authorizer.Authorize(context.Interaction.User.Id, Permission.EditPhotography, venue)
            .Authorized;
        if (!canPhotograph)
        {
            await context.Interaction.Channel.SendMessageAsync("Sorry, you do not have permission to edit the photography for this venue! 🥲");
            return;
        }
        
        var broadcast = await repository.GetByIdAsync<BroadcastReceipt>(broadcastId);
        var responder = context.Interaction.User;
        
        foreach (var broadcastMessage in broadcast.BroadcastMessages)
        {
            if (broadcastMessage.Status != MessageStatus.Sent) continue;
            var newMessage = "You're handling this. 🥳";
            if (broadcastMessage.UserId != responder.Id)
                newMessage = $"{context.Interaction.User.Username} is handling this. 🥰";

            var channel = await client.GetChannelAsync(broadcastMessage.ChannelId) as IDMChannel;
            channel?.ModifyMessageAsync(broadcastMessage.MessageId, props =>
            {
                props.Components = new ComponentBuilder().Build();
                props.Embeds = new[]
                {
                    venueRenderer.Render(venue).Build(),
                    new EmbedBuilder().WithDescription(newMessage).Build()
                };
            });
        }
        
        await context.Interaction.Channel.SendMessageAsync("Have fun with it! 💕");
    }
}