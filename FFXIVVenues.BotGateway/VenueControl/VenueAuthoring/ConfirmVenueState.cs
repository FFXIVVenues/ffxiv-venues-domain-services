using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.GuildEngagement;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Infrastructure.Presence;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueAuditing;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueApproval;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueEditing.SessionStates;
using FFXIVVenues.Veni.VenueEvents;
using FFXIVVenues.Veni.VenueRendering;
using FFXIVVenues.VenueModels;
using FFXIVVenues.VenueService.Client.Events;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring;

class ConfirmVenueSessionState(
    IVenueRenderer venueRenderer,
    IApiService apiService,
    IVenueApprovalService indexersService,
    IGuildManager guildManager,
    IAuthorizer authorizer,
    IRepository repository,
    IDiscordClient discordClient,
    IVenueAuditService auditService,
    IActivityManager activityManager,
    UiConfiguration uiConfig)
    : ISessionState
{
    private static string[] _preexisingResponse = new[]
    {
        "Wooo! All updated!",
        "All done for you! 🥳",
        "Ok, that's updated for you! 😊"
    };

    private static string[] _summaryResponse = new[]
    {
        "Here's a preview of your venue!",
        "Okay! 🙌 Here's what your venue will look like!",
        "Nice! So, how does it look? 😊"
    };

    private static string[] _workingOnItResponse = new[]
    {
        "Yaaay! I'm working on it! So excited. 🥳",
        "Woo, working on it! This bit is always so exciting. 🎉",
        "Alright, working on it! 😊"
    };

    private static string[] _successfulNewResponse = new[]
    {
        "Wooo! I've sent it. Once it's approved, it'll show on the index!",
        "All done! Once an Indexer approves it, it'll be live! 🥳",
        "Ok! We'll get that approved and get it live soon! 🎉"
    };

    public async Task Enter(VeniInteractionContext c)
    {
        var bannerUrl = c.Session.GetItem<string>(SessionKeys.BANNER_URL);
        var modifying = c.Session.InEditing();
        var venue = c.Session.GetVenue();
            
        await c.Interaction.RespondAsync(_summaryResponse.PickRandom(),
            embed: (await venueRenderer.ValidateAndRenderAsync(venue, bannerUrl)).Build(),
            component: new ComponentBuilder()
                .WithButton("Looks perfect! Save!", c.Session.RegisterComponentHandler(this.LooksPerfect, ComponentPersistence.ClearRow), ButtonStyle.Success)
                .WithButton(modifying ? "Edit more" : "Edit", c.Session.RegisterComponentHandler(this.Edit, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .WithButton("Cancel", c.Session.RegisterComponentHandler(this.Cancel, ComponentPersistence.ClearRow), ButtonStyle.Danger)
                .Build());
    }

    private async Task LooksPerfect(ComponentVeniInteractionContext c)
    {
        var isNewVenue = c.Session.IsNewVenue();
        var modifying = c.Session.InEditing();

        _ = c.Interaction.Channel.SendMessageAsync(_workingOnItResponse.PickRandom());
        _ = c.Interaction.Channel.TriggerTypingAsync();
            
        var venue = c.Session.GetVenue();
        var bannerUrl = c.Session.GetItem<string>(SessionKeys.BANNER_URL);
        var isApprover = authorizer
            .Authorize(c.Interaction.User.Id, Permission.ApproveVenue, venue)
            .Authorized;
            
        var uploadVenueResponse = await apiService.PutVenueAsync(venue);
        if (!uploadVenueResponse.IsSuccessStatusCode)
        {
            _ = c.Interaction.Channel.SendMessageAsync("Ooops! Something went wrong. 😢");
            await c.Interaction.Channel.SendMessageAsync(_summaryResponse.PickRandom(),
                embed: (await venueRenderer.ValidateAndRenderAsync(venue, bannerUrl)).Build(),
                components: new ComponentBuilder()
                    .WithButton("Looks perfect! Save!", c.Session.RegisterComponentHandler(this.LooksPerfect, ComponentPersistence.ClearRow), ButtonStyle.Success)
                    .WithButton(modifying ? "Edit more" : "Edit", c.Session.RegisterComponentHandler(this.Edit, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Cancel", c.Session.RegisterComponentHandler(this.Cancel, ComponentPersistence.ClearRow), ButtonStyle.Danger)
                    .Build());
            return;
        }
            
        if (bannerUrl != null) // changed
            await apiService.PutVenueBannerAsync(venue.Id, bannerUrl);

        if (isNewVenue)
        {
            new VenueCreatedHandler(repository, discordClient, apiService, uiConfig).Handle(
                new VenueCreatedEvent(venue.Id, c.Interaction.User.Id));
        }
        else
        {
            new VenueEditedHandler(repository, discordClient, apiService, uiConfig).Handle(
                new VenueEditEvent(venue.Id, c.Interaction.User.Id));
        }
        
        if (!isNewVenue)
        {
            _ = guildManager.SyncRolesForVenueAsync(venue);
            _ = guildManager.FormatDisplayNamesForVenueAsync(venue);
            await c.Interaction.Channel.SendMessageAsync(_preexisingResponse.PickRandom());
            var latestAudit = await auditService.GetLatestRecordFor(venue);
            if (latestAudit?.Status is VenueAuditStatus.Failed or VenueAuditStatus.Pending or VenueAuditStatus.AwaitingResponse)
                await auditService.UpdateAuditStatus(latestAudit, venue, c.Interaction.User.Id, VenueAuditStatus.EditedLater);
        }
        else if (isApprover)
        {
            var success = await indexersService.ApproveVenueAsync(venue, c.Interaction.User.Id);
            if (success)
                await c.Interaction.Channel.SendMessageAsync("All done and auto-approved for you. :heart:");
            else
                await c.Interaction.Channel.SendMessageAsync("Something, went wrong while trying to auto-approve it for you. 😢");
        }
        else
        {
            await c.Interaction.Channel.SendMessageAsync(_successfulNewResponse.PickRandom());
            await SendToApprovers(venue, bannerUrl);
        }

       
        _ = c.Session.ClearStateAsync(c);
        _ = activityManager.UpdateActivityAsync();
    }

    private Task Edit(ComponentVeniInteractionContext c) =>
        c.Session.MoveStateAsync<EditVenueSessionState>(c);

    private Task Cancel(ComponentVeniInteractionContext c)
    {
        _ = c.Session.ClearStateAsync(c);
        return c.Interaction.Channel.SendMessageAsync("It's as if it never happened! 😅");
    }

    private Task SendToApprovers(Venue venue, string bannerUrl) =>
        indexersService.SendForApproval(venue, bannerUrl);

}