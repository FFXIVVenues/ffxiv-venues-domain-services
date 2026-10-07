using Discord;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Authorisation;
using FFXIVVenues.BotGateway.Infrastructure.Components;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.BotGateway.VenueControl;
using FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.VenueEditing.SessionStates;
using FFXIVVenues.BotGateway.VenueControl.VenueClosing.SessionStates;
using FFXIVVenues.FlagService.Client;
using System.Threading.Tasks;

namespace FFXIVVenues.BotGateway.VenueEvents.VenueFlags.Responses;

public class TemporarilyCloseVenueFlagHandler(FlagServiceClient flagServiceClient, IRepository repository, IApiService apiService, IAuthorizer authorizer) : IComponentHandler
{
    public static string Key => "FLAG_RESPONSE_TEMP_CLOSE_VENUE";

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

        await flagServiceClient.ResolveFlagAsync(flagId, userId);

        context.Session.SetVenue(venue);
        await context.Session.MoveStateAsync<CloseEntryState>(context);
    }
}