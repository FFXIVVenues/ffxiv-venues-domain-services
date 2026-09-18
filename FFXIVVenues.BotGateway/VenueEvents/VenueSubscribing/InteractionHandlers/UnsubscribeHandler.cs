using Discord;
using FFXIVVenues.BotGateway.Infrastructure.Components;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.BotGateway.VenueEvents.VenueSubscribing.Models;
using FFXIVVenues.DomainData.Context;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FFXIVVenues.BotGateway.VenueEvents.VenueSubscribing.Handlers;

public class UnsubscribeHandler(DomainDataContext db) : IComponentHandler
{

    public static string Key => "VENUE_UNSUBCRIBE";
    
    public async Task HandleAsync(ComponentVeniInteractionContext context, string[] args)
    {
        _ = context.Interaction.ModifyOriginalResponseAsync(props =>
            props.Components = new ComponentBuilder().Build());

        var user = context.Interaction.User.Id;
        var venueId = args[0];

        var existingSub = await db.Favorites.FindAsync(context.Interaction.User.Id, args[0]);
        if (existingSub is null)
        {
            await context.Interaction.FollowupAsync(SubscriptionStrings.NotSubscribed, ephemeral: true);
            return;
        }

        db.Favorites.Remove(existingSub);
        await db.SaveChangesAsync();

        await context.Interaction.FollowupAsync(SubscriptionStrings.Unsubscribed, ephemeral: true);
    }
    
}