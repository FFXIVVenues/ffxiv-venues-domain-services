using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.GuildEngagement;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueAuditing;
using FFXIVVenues.Veni.VenueEvents;
using FFXIVVenues.VenueModels;
using FFXIVVenues.VenueService.Client.Events;

namespace FFXIVVenues.Veni.VenueControl.VenueDeletion.SessionStates;

class DeleteVenueSessionState(IRepository repository, IDiscordClient client, IApiService apiService, IVenueAuditService auditService, IAuthorizer authorizer, IGuildManager guildManager)
    : ISessionState
{

    private static string[] _messages = new[]
    {
        "Are you super sure you want to **delete {0}**? 😢",
        "Are you really sure you want to **delete {0}**?",
        "R-really? Are you sure you want me to **delete {0}**?"
    };

    private static string[] _deleteMessages = new[]
    {
        "Okay, that's done. 😢",
        "It's gone. 😢"
    };

    private Venue _venue;

    public Task Enter(VeniInteractionContext c)
    {
        this._venue = c.Session.GetVenue();
        return c.Interaction.RespondAsync(string.Format(_messages.PickRandom(), _venue.Name), new ComponentBuilder()
            .WithButton("Yes, delete it 😢", c.Session.RegisterComponentHandler(
                async cm =>
                {
                    var authorize = authorizer.Authorize(cm.Interaction.User.Id, Permission.DeleteVenue, _venue);
                    if (!authorize.Authorized)
                    {
                        await cm.Interaction.Channel.SendMessageAsync(
                            "Sorry, you do not have permission to delete this venue. 😢");
                        return;
                    }

                    _ = cm.Interaction.Channel.SendMessageAsync(_deleteMessages.PickRandom());
                    await apiService.DeleteVenueAsync(_venue.Id);
                    var latestAudit = await auditService.GetLatestRecordFor(this._venue);
                    if (latestAudit?.Status is VenueAuditStatus.Failed or VenueAuditStatus.Pending or VenueAuditStatus.AwaitingResponse)
                        await auditService.UpdateAuditStatus(latestAudit, this._venue, c.Interaction.User.Id, VenueAuditStatus.DeletedLater);
                    await guildManager.SyncRolesForVenueAsync(_venue);
                    
                    new VenueDeletedHandler(repository, client).Handle(
                        new VenueDeletedEvent(_venue.Id, _venue.Name, cm.Interaction.User.Id));
                },
                ComponentPersistence.ClearRow), ButtonStyle.Danger)
            .WithButton("No, don't! I've changed my mind. 🙂", c.Session.RegisterComponentHandler(cm => cm.Interaction.Channel.SendMessageAsync("Phew 😅"), ComponentPersistence.ClearRow))
            .Build());
    }
}