using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueControl.VenueDeletion.SessionStates;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueClosing.SessionStates;

internal class CloseHowLongWhenEntryState(IApiService apiService, IAuthorizer authorizer) : ISessionState
{
    private Venue _venue;

    public Task Enter(VeniInteractionContext c)
    {
        this._venue = c.Session.GetVenue();
        var component = this.BuildOpenComponent(c);
        return c.Interaction.RespondAsync(VenueControlStrings.AskForHowLongClosingFor, component.Build()); //change text later
    }

    private ComponentBuilder BuildOpenComponent(VeniInteractionContext c)
    {
        var selectComponent = new SelectMenuBuilder()
            .WithCustomId(c.Session.RegisterComponentHandler(OnSelect, ComponentPersistence.ClearRow));

        selectComponent.AddOption("The next 18 hours", "18")
            .AddOption("The next 2 days", "48")
            .AddOption("The next 3 days", "72")
            .AddOption("The next 5 days", "120")
            .AddOption("The next 7 days", "168")
            .AddOption("The next 2 weeks", "336")
            .AddOption("The next 3 weeks", "504")
            .AddOption("The next 4 weeks", "672")
            .AddOption("The next 6 weeks", "1008")
            .AddOption("The next 2 months", "1344")
            .AddOption("The next 3 months", "2016");
            
        if (c.Session.GetItem<DateTimeOffset>(SessionKeys.CLOSING_DATE) == default)
            selectComponent.AddOption("Permanently (delete)", "perm");
        return new ComponentBuilder().WithSelectMenu(selectComponent).WithBackButton(c);
    }

    private async Task OnSelect(ComponentVeniInteractionContext c)
    {
        var value = c.Interaction.Data.Values.Single();
        if (value == "perm")
        {
            await c.Session.MoveStateAsync<DeleteVenueSessionState>(c);
            return;
        }
        
        var authorize = authorizer.Authorize(c.Interaction.User.Id, Permission.OpenVenue, _venue);
        if (!authorize.Authorized)
        {
            await c.Interaction.Channel.SendMessageAsync(
                "Sorry, you do not have permission to close this venue. 😢");
            return;
        }
        
        var until = int.Parse(c.Interaction.Data.Values.Single());
        

        var closingDate = c.Session.GetItem<DateTimeOffset>(SessionKeys.CLOSING_DATE);
        if (closingDate != default)
        {
            var closingTime = c.Session.GetItem<int>(SessionKeys.CLOSING_HOUR);
            var from = closingDate.AddHours(closingTime);
            await apiService.CloseVenueAsync(this._venue.Id, from, from.AddHours(until));
            await c.Interaction.Channel.SendMessageAsync(VenueControlStrings.VenueNowClosed);
        }
        else
        {
            await apiService.CloseVenueAsync(this._venue.Id, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(until));
            await c.Interaction.Channel.SendMessageAsync(VenueControlStrings.VenueNowClosed);
        }
            
        _ = c.Session.ClearStateAsync(c);
    }
}