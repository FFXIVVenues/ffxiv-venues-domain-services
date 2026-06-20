using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.GuildEngagement;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.Utils.Broadcasting;
using FFXIVVenues.Veni.VenueEvents;
using FFXIVVenues.Veni.VenueRendering;
using FFXIVVenues.VenueModels;
using FFXIVVenues.VenueService.Client.Events;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueApproval;

public class VenueApprovalService(
    IRepository repository,
    DiscordSocketClient client,
    IVenueRenderer venueRenderer,
    IApiService apiService,
    UiConfiguration uiConfiguration,
    NotificationsConfiguration config,
    IAuthorizer authorizer,
    IGuildManager guildManager)
    : IVenueApprovalService
{
    private readonly ConcurrentDictionary<string, Broadcast> _broadcasts = new();

    public async Task<BroadcastReceipt> SendForApproval(Venue venue, string bannerUrl)
    {
        var dc = FfxivWorlds.GetRegionForDataCenter(venue.Location?.DataCenter);
        var recipients = config.Approvals.ResolveFor(dc);
        var broadcast = new Broadcast(Guid.NewGuid().ToString(), client);
        _broadcasts[broadcast.Id] = broadcast;
        return await broadcast
            .WithMessage($"Heyo indexers!\nVenue '**{venue.Name}**' ({venue.Id}) needs approving! :heart:")
            .WithEmbed(await venueRenderer.ValidateAndRenderAsync(venue, bannerUrl))
            .WithComponent(bcc =>
            {
                ComponentBuilder approveRejectComponent = null;
                var approveHandler = bcc.RegisterComponentHandler(c => this.ApproveVenueButtonHandler(venue, c));
                var rejectHandler = bcc.RegisterComponentHandler(c => this.DeleteVenueButtonHandler(approveRejectComponent, venue, bcc, c));
                return approveRejectComponent = new ComponentBuilder()
                    .WithButton("Approve", approveHandler, ButtonStyle.Success)
                    .WithButton("Reject", rejectHandler, ButtonStyle.Secondary);
            })
            .SendToAsync(recipients);
    }
        
    private async Task ApproveVenueButtonHandler(Venue venue, Broadcast.BroadcastInteractionContext approveBic)
    {
        var canApprove = authorizer
            .Authorize(approveBic.CurrentUser.Id, Permission.ApproveVenue, venue)
            .Authorized;
        if (!canApprove)
        {
            await approveBic.Component.Channel.SendMessageAsync("Sorry, you do not have permission to approve this venue! 🥲");
            return;
        }

        _ = approveBic.ModifyForOtherUsers((props, original) =>
        {
            props.Components = new ComponentBuilder().Build();
            props.Embeds = original.Embeds.Select(e => (Embed)e)
                .Concat(new[] { new EmbedBuilder().WithDescription($"{approveBic.CurrentUser.Username} handled this and approved the venue. 🥳").Build() })
                .ToArray();
        });
        await approveBic.ModifyForCurrentUser((props, original) =>
        {
            props.Components = new ComponentBuilder().Build();
            props.Embeds = original.Embeds.Select(e => (Embed)e)
                .Concat(new[] { new EmbedBuilder().WithDescription($"You handled this and approved the venue. 🥳").Build() })
                .ToArray();
        });

        await approveBic.Component.Channel.SendMessageAsync("Wew! Thank you, I've let them know! 💕");
            
        await ApproveVenueAsync(venue, approveBic.CurrentUser.Id);
    }

    public async Task<bool> ApproveVenueAsync(Venue venue, ulong approver)
    {
        // It may have been edited by indexers, so get the latest.
        venue = await apiService.GetVenueAsync(venue.Id);
        var response = await apiService.ApproveAsync(venue.Id);
        if (!response.IsSuccessStatusCode)
            return false;
            
        _ = guildManager.AssignRolesForVenueAsync(venue);
        _ = guildManager.FormatDisplayNamesForVenueAsync(venue);

        foreach (var managerId in venue.Managers)
        {
            var manager = await client.GetUserAsync(ulong.Parse(managerId));
            var dmChannel = await manager.CreateDMChannelAsync();
            _ = dmChannel.SendMessageAsync($"Hey hey! :heart:\n**{venue.Name}** has been **approved** and it's live!\n{uiConfiguration.BaseUrl}/venue/{venue.Id}\n" +
                                           $"I've assigned you your Venue Manager discord role too.\n" +
                                           $"Let me know if you'd like anything edited or anything you'd like help with. 🥳",
                embed: (await venueRenderer.ValidateAndRenderAsync(venue)).Build());
        }
        
        new VenueApprovedHandler(repository, client, apiService, uiConfiguration).Handle(
            new VenueApprovedEvent(venue.Id, approver));

        return true;
    }

    private async Task DeleteVenueButtonHandler(ComponentBuilder approveRejectComponent, 
        Venue venue, 
        Broadcast.ComponentContext bcc, 
        Broadcast.BroadcastInteractionContext rejectBic)
    {
        var isApprover = authorizer
            .Authorize(rejectBic.CurrentUser.Id, Permission.ApproveVenue, venue)
            .Authorized;
        if (!isApprover)
        {
            await rejectBic.Component.RespondAsync("Sorry, only indexers can do this! :sad:");
            return;
        }

        var confirmDeleteHandler = bcc.RegisterComponentHandler(async confirmDeleteBic =>
        {
            await apiService.DeleteVenueAsync(venue.Id);

            _ = rejectBic.ModifyForOtherUsers((props, original) =>
            {
                props.Components = new ComponentBuilder().Build();
                props.Embeds = original.Embeds.Select(e => (Embed)e)
                    .Concat(new[] { new EmbedBuilder().WithDescription($"{rejectBic.CurrentUser.Username} handled this and rejected the venue.").Build() })
                    .ToArray();
            });

            await rejectBic.ModifyForCurrentUser((props, original) =>
            {
                props.Components = new ComponentBuilder().Build();
                props.Embeds = original.Embeds.Select(e => e as Embed)
                    .Concat(new[] { new EmbedBuilder().WithDescription($"You handled this and rejected the venue.").Build() })
                    .ToArray();
            });
                
            await rejectBic.Component.Channel.SendMessageAsync("Okay! Well, maybe the next one then. 😢");
            
            new VenueDeletedHandler(repository, client).Handle(
                new VenueDeletedEvent(venue.Id, venue.Name, confirmDeleteBic.CurrentUser.Id));
        });

        var cancelHandler = bcc.RegisterComponentHandler(cancelBic =>
            cancelBic.ModifyForCurrentUser((props, original) =>
                props.Components = approveRejectComponent.Build()));

        await rejectBic.ModifyForCurrentUser((props, original) =>
            props.Components = new ComponentBuilder()
                .WithButton("Yes, delete venue", confirmDeleteHandler, ButtonStyle.Danger)
                .WithButton("No, cancel", cancelHandler, ButtonStyle.Secondary)
                .Build());
    }

    public async Task<bool> HandleComponentInteractionAsync(SocketMessageComponent context)
    {
        foreach (var broadcast in _broadcasts)
        {
            var handled = await broadcast.Value.HandleComponentInteraction(context);
            if (handled) return true;
        }
        return false;
    }

}