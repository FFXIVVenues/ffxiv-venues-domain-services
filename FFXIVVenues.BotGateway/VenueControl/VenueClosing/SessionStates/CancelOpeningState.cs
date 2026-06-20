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

namespace FFXIVVenues.Veni.VenueControl.VenueClosing.SessionStates;

internal class CancelOpeningState(IApiService apiService, IAuthorizer authorizer) : ISessionState
{

    private List<(Opening Opening, bool IsOverride)> _openings = new();
    
    public async Task Enter(VeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();

        foreach (var schedule in venue.Schedule)
        {
            var enumerator = new ScheduleEnumerator(schedule, DateTimeOffset.UtcNow);
            var i = 0;
            while (enumerator.MoveNext() && i < 7)
            {
                var current = enumerator.Current;
                if (current!.Start < DateTimeOffset.UtcNow)
                    continue;
                _openings.Add((current, false));
                i++;
            }
            i++;
        }

        foreach (var schedule in venue.ScheduleOverrides.Where(o => o.Open && o.Start > DateTimeOffset.UtcNow))
            _openings.Add((new (schedule.Start, schedule.End), true));

        _openings = _openings.OrderBy(o => o.Opening.Start).Take(25).ToList();

        if (_openings.Count > 1)
        {
            var selectComponent = new SelectMenuBuilder()
                .WithCustomId(c.Session.RegisterComponentHandler(OnSelect, ComponentPersistence.ClearRow));
            var i = 0;
            foreach (var opening in _openings)
            {
                var label = opening.Opening.Start.ToString("dddd dd MMMM");
                if (opening.IsOverride) label += " (Adhoc)";
                selectComponent.AddOption(label, i.ToString());
                i++;
            }
            var componentBuilder = new ComponentBuilder().WithSelectMenu(selectComponent);
            await c.Interaction.Channel.SendMessageAsync(VenueControlStrings.AskForOpeningToCancel, components: componentBuilder.WithBackButton(c).Build());
            return;
        }

        var authorize = authorizer.Authorize(c.Interaction.User.Id, Permission.CloseVenue, venue);
        if (!authorize.Authorized)
        {
            await c.Interaction.Channel.SendMessageAsync(
                "Sorry, you do not have permission to close this venue. 😢");
            return;
        }
        
        await this.CancelAsync(venue, 0);
        
        await c.Interaction.Channel.SendMessageAsync(VenueControlStrings.VenueOpeningCancelled);
        await c.Session.ClearStateAsync(c);
    }

    private async Task OnSelect(ComponentVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        var authorize = authorizer.Authorize(c.Interaction.User.Id, Permission.CloseVenue, venue);
        if (!authorize.Authorized)
        {
            await c.Interaction.Channel.SendMessageAsync(
                "Sorry, you do not have permission to close this venue. 😢");
            return;
        }

        var indexSelected = int.Parse(c.Interaction.Data.Values.Single());
        await this.CancelAsync(c.Session.GetVenue(), indexSelected);
        
        await c.Interaction.Channel.SendMessageAsync(VenueControlStrings.VenueOpeningCancelled);
        await c.Session.ClearStateAsync(c);
    }

    private Task CancelAsync(Venue venue, int index)
    {
        
        
        var opening = this._openings.Skip(index).FirstOrDefault();
        if (opening.IsOverride)
            return apiService.RemoveOverridesAsync(venue.Id, opening.Opening.Start, opening.Opening.End);
        
        return apiService.CloseVenueAsync(venue.Id, opening.Opening.Start, opening.Opening.End);
    }
    
}

