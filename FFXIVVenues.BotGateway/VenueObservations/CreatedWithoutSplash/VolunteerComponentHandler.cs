using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils.Broadcasting;
using FFXIVVenues.Veni.VenueAuditing;
using FFXIVVenues.Veni.VenueRendering;
using OfficeOpenXml.ConditionalFormatting;

namespace FFXIVVenues.Veni.VenueObservations.CreatedWithoutSplash;

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