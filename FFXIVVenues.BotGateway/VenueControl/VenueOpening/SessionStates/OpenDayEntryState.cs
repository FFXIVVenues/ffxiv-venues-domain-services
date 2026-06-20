using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Context.SessionHandling;
using FFXIVVenues.BotGateway.Utils;
using FFXIVVenues.BotGateway.VenueControl;

namespace FFXIVVenues.BotGateway.VenueControl.VenueOpening.SessionStates;

internal class OpenDayEntryState : ISessionState
{
    public Task Enter(VeniInteractionContext c)
    {
        var component = this.BuildOpenComponent(c);
        return c.Interaction.RespondAsync(VenueControlStrings.AskForDayOfOpening, component.WithBackButton(c).Build()); //change text later
    }

    private ComponentBuilder BuildOpenComponent(VeniInteractionContext c)
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
        c.Session.SetItem(SessionKeys.OPENING_DATE, DateTimeOffset.Parse(date));
        return c.Session.MoveStateAsync<OpenTimeEntryState>(c);
    }
    
}

