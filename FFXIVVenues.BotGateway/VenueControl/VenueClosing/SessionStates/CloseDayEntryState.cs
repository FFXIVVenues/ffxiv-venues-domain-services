using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueClosing.SessionStates;

internal class CloseDayEntryState : ISessionState
{
    public Task Enter(VeniInteractionContext c)
    {
        var component = this.BuildCloseComponent(c);
        return c.Interaction.RespondAsync(VenueControlStrings.AskForDayOfClosing, component.WithBackButton(c).Build()); //change text later
    }

    private ComponentBuilder BuildCloseComponent(VeniInteractionContext c)
    {
        var timezone = c.Session.GetItem<string>(SessionKeys.TIMEZONE_ID);
        var selectComponent = new SelectMenuBuilder()
            .WithCustomId(c.Session.RegisterComponentHandler(OnSelect, ComponentPersistence.ClearRow));
        foreach (var date in DateHelper.GetNextNDates(21, timezone))
            selectComponent.AddOption(date.ToString("dddd dd MMMM"), date.ToString());
        return new ComponentBuilder().WithSelectMenu(selectComponent);
    }

    private Task OnSelect(ComponentVeniInteractionContext c)
    {
        var date = c.Interaction.Data.Values.Single(); 
        c.Session.SetItem(SessionKeys.CLOSING_DATE, DateTimeOffset.Parse(date));
        return c.Session.MoveStateAsync<CloseTimeEntryState>(c);
    }
}

