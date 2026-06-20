using System;
using System.Collections.Generic;
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

internal class CancelClosureState(IApiService apiService, IAuthorizer authorizer) : ISessionState
{

    private List<ScheduleOverride> _overrides;
    
    public async Task Enter(VeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();

        this._overrides = venue.ScheduleOverrides.Where(o => !o.Open && o.Start > DateTime.UtcNow).ToList();
        if (this._overrides.Count > 1)
        {
            var selectComponent = new SelectMenuBuilder()
                .WithCustomId(c.Session.RegisterComponentHandler(OnSelect, ComponentPersistence.ClearRow));
            var i = 0;
            foreach (var @override in this._overrides)
            {
                selectComponent.AddOption(@override.Start.ToString("dddd dd MMMM"), i.ToString());
                i++;
            }
            var componentBuilder = new ComponentBuilder().WithSelectMenu(selectComponent);
            await c.Interaction.Channel.SendMessageAsync(VenueControlStrings.AskForClosureToCancel, components: componentBuilder.WithBackButton(c).Build());
            return;
        }

        var authorize = authorizer.Authorize(c.Interaction.User.Id, Permission.OpenVenue, venue);
        if (!authorize.Authorized)
        {
            await c.Interaction.Channel.SendMessageAsync(
                "Sorry, you do not have permission to close this venue. 😢");
            return;
        }
        
        await this.CancelAsync(venue, 0);
        
        await c.Interaction.Channel.SendMessageAsync(VenueControlStrings.VenueClosureCancelled);
        await c.Session.ClearStateAsync(c);
    }

    private async Task OnSelect(ComponentVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        var authorize = authorizer.Authorize(c.Interaction.User.Id, Permission.OpenVenue, venue);
        if (!authorize.Authorized)
        {
            await c.Interaction.Channel.SendMessageAsync(
                "Sorry, you do not have permission to close this venue. 😢");
            return;
        }
        
        var indexSelected = int.Parse(c.Interaction.Data.Values.Single());
        await this.CancelAsync(c.Session.GetVenue(), indexSelected);
        
        await c.Interaction.Channel.SendMessageAsync(VenueControlStrings.VenueClosureCancelled);
        await c.Session.ClearStateAsync(c);
    }

    private Task CancelAsync(Venue venue, int index)
    {
        var closure = this._overrides.Skip(index).FirstOrDefault();
        if (closure is not null)
            return apiService.RemoveOverridesAsync(venue.Id, closure.Start, closure.End);
        return Task.CompletedTask;
    }
    
}

