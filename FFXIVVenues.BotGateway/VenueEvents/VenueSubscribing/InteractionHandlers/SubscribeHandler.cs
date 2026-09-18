using Discord;
using FFXIVVenues.BotGateway.Infrastructure.Components;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.BotGateway.VenueEvents.VenueSubscribing.Models;
using FFXIVVenues.DomainData.Context;
using FFXIVVenues.DomainData.Entities.Favorites;
using System.Threading.Tasks;

namespace FFXIVVenues.BotGateway.VenueEvents.VenueSubscribing.Handlers;

public class SubscribeHandler(DomainDataContext db) : IComponentHandler
{

    public static string Key => "VENUE_SUBCRIBE";
    
    public async Task HandleAsync(ComponentVeniInteractionContext context, string[] args)
    {
        _ = context.Interaction.ModifyOriginalResponseAsync(props =>
            props.Components = new ComponentBuilder().Build());

        var user = context.Interaction.User.Id;
        var venueId = args[0];

        var existingFavorite = await db.Favorites.FindAsync(user, venueId);
        if (existingFavorite != null)
        {
            await context.Interaction.FollowupAsync(SubscriptionStrings.AlreadySubscribed, ephemeral: true);
            return;
        }

        var favorite = new Favorite
        {
            UserId = user,
            VenueId = venueId
        };
        await db.Favorites.AddAsync(favorite);
        await db.SaveChangesAsync();

        await context.Interaction.FollowupAsync(SubscriptionStrings.Subscribed, ephemeral: true);
    }

}