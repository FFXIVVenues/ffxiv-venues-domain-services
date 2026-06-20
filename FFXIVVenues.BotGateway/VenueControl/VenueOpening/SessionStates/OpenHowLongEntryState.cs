using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueOpening.SessionStates;

internal class OpenHowLongWhenEntryState(IApiService apiService, IAuthorizer authorizer) : ISessionState
{
    private Venue _venue;

    public Task Enter(VeniInteractionContext c)
    {
        this._venue = c.Session.GetVenue();
        var component = this.BuildOpenComponent(c);
        return c.Interaction.RespondAsync(VenueControlStrings.AskForHowLongOpeningFor, component.Build()); //change text later
    }

    private ComponentBuilder BuildOpenComponent(VeniInteractionContext c)
    {
        var selectComponent = new SelectMenuBuilder()
            .WithCustomId(c.Session.RegisterComponentHandler(OnSelect, ComponentPersistence.ClearRow));

        selectComponent.AddOption("The next hour", "1")
            .AddOption("The next 2 hours", "2")
            .AddOption("The next 3 hours", "3")
            .AddOption("The next 4 hours", "4")
            .AddOption("The next 5 hours", "5")
            .AddOption("The next 6 hours", "6")
            .AddOption("The next 7 hours", "7");
        return new ComponentBuilder().WithSelectMenu(selectComponent).WithBackButton(c);
    }

    private async Task OnSelect(ComponentVeniInteractionContext c)
    {
        var authorize = authorizer.Authorize(c.Interaction.User.Id, Permission.OpenVenue, _venue);
        if (!authorize.Authorized)
        {
            await c.Interaction.Channel.SendMessageAsync(
                "Sorry, you do not have permission to open this venue. 😢");
            return;
        }
        
        var until = int.Parse(c.Interaction.Data.Values.Single());
        

        var openingDate = c.Session.GetItem<DateTimeOffset>(SessionKeys.OPENING_DATE);
        if (openingDate != default)
        {
            var openingTime = c.Session.GetItem<int>(SessionKeys.OPENING_HOUR);
            var from = openingDate.AddHours(openingTime);
            await apiService.OpenVenueAsync(this._venue.Id, from, from.AddHours(until));
            await c.Interaction.Channel.SendMessageAsync(VenueControlStrings.VenueOpenLater);
        }
        else
        {
            await apiService.OpenVenueAsync(this._venue.Id, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(until));
            await c.Interaction.Channel.SendMessageAsync(VenueControlStrings.VenueNowOpen);
        }
            
        _ = c.Session.ClearStateAsync(c);
    }
}