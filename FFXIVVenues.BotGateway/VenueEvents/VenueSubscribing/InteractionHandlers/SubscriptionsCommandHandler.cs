using Discord;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Infrastructure.Commands;
using FFXIVVenues.BotGateway.Infrastructure.Commands.Attributes;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Context.SessionHandling;
using FFXIVVenues.BotGateway.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.BotGateway.Utils;
using FFXIVVenues.BotGateway.VenueControl;
using FFXIVVenues.BotGateway.VenueDiscovery.SessionStates;
using FFXIVVenues.BotGateway.VenueEvents.VenueSubscribing.Models;
using FFXIVVenues.BotGateway.VenueRendering;
using FFXIVVenues.DomainData.Context;
using FFXIVVenues.DomainData.Mapping;
using FFXIVVenues.VenueModels;
using JasperFx.Events;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVVenues.BotGateway.VenueEvents.VenueSubscribing.InteractionHandlers;

[DiscordCommand("subscriptions", "See your venue subscriptions.")]
internal class SubscriptionsCommandHandler(DomainDataContext db, IMapFactory mapFactory, IVenueRenderer venueRenderer) : ICommandHandler
{

    private IEnumerable<Venue> _subscribedVenues;

    public async Task HandleAsync(SlashCommandVeniInteractionContext interactionContext)
    {
        var userId = interactionContext.Interaction.User.Id;
        var subscriptions = db.Favorites.Where(s => s.UserId == userId && s.Venue.Deleted == null).Include(s => s.Venue.Location);

        if (!subscriptions.Any())
        {
            await interactionContext.Interaction.RespondAsync(SubscriptionStrings.NoSubscriptions, ephemeral: true);
            return;
        }

        var selectMenuKey = interactionContext.Session.RegisterComponentHandler(this.HandleSelection, ComponentPersistence.DeleteMessage);
        var componentBuilder = new ComponentBuilder();
        var selectMenuBuilder = new SelectMenuBuilder() { CustomId = selectMenuKey };
        foreach (var sub in subscriptions.OrderBy(v => v.Venue.Name))
        {
            var selectMenuOption = new SelectMenuOptionBuilder
            {
                Label = sub.Venue.Name,
                Description = venueRenderer.RenderLocationString(sub.Venue.Location),
                Value = sub.Venue.Id
            };
            selectMenuBuilder.AddOption(selectMenuOption);
        }
        componentBuilder.WithSelectMenu(selectMenuBuilder);
        await interactionContext.Interaction.RespondAsync(SubscriptionStrings.SubscribedVenues, components: componentBuilder.Build());
    }


    public async Task HandleSelection(ComponentVeniInteractionContext context)
    {
        _ = context.Interaction.ModifyOriginalResponseAsync(props =>
            props.Components = new ComponentBuilder().Build());

        var selectedVenueId = context.Interaction.Data.Values.Single();
        var user = context.Interaction.User.Id;
        var venue = await db.Venues.FindAsync(selectedVenueId);
        var venueDto = mapFactory.GetModelMapper().Map<Venue>(venue);

        await context.Session.ClearStateAsync(context);

        var render = await venueRenderer.ValidateAndRenderAsync(venueDto);
        var actions = await venueRenderer.RenderActionComponentsAsync(context, venueDto, user);
        await context.Interaction.FollowupAsync(embed: render.Build(), components: actions.Build());
    }

}
