using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using Newtonsoft.Json.Linq;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates;

class ManagerEntrySessionState(IAuthorizer authorizer) : ISessionState
{
    public Task Enter(VeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();

        if (!authorizer.Authorize(c.Interaction.User.Id, Permission.EditManagers, venue).Authorized)
            return c.Session.MoveStateAsync<ConfirmVenueSessionState>(c);

        c.Session.RegisterMessageHandler(this.OnMessageReceived);
        return c.Interaction.RespondAsync("Who is/are the manager(s)? :heart:",
            new ComponentBuilder()
                .WithBackButton(c)
                .WithSkipButton<ConfirmVenueSessionState, ConfirmVenueSessionState>(c)
                .Build());
    }

    public Task OnMessageReceived(MessageVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();

        var regex = new Regex("[0-9]{17,}");
        var input = c.Interaction.Content.StripMentions(c.Client.CurrentUser.Id);
        var discordIds = regex.Matches(input).Select(m => m.Value).ToList();

        if (discordIds is not { Count: > 0})
            return c.Interaction.Channel.SendMessageAsync(MessageRepository.DontUnderstandResponses.PickRandom());

        venue.Managers = discordIds;
        return c.Session.MoveStateAsync<ConfirmVenueSessionState>(c);
    }

}