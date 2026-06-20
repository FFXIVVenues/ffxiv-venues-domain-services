using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueControl.VenueClosing.SessionStates;

namespace FFXIVVenues.Veni.VenueControl.VenueOpening.SessionStates;

internal class OpenEntryState : ISessionState
{
    public Task Enter(VeniInteractionContext c)
    {
        var component = this.BuildOpenComponent(c);
        return c.Interaction.RespondAsync(VenueControlStrings.AskIfOpeningIsNowOrLater, component.Build());
    }

    private ComponentBuilder BuildOpenComponent(VeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        var isOpen = venue.Resolution?.IsNow ?? false;
        var isClosed = venue.ScheduleOverrides.Any(s => s.IsNow && s.Open is false);
        var hasFutureClosure = venue.ScheduleOverrides.Any(s => !s.Open && s.Start > DateTime.UtcNow);
        var selectComponent = new SelectMenuBuilder()
            .WithCustomId(c.Session.RegisterComponentHandler(OnSelect, ComponentPersistence.ClearRow));

        if (isOpen)
            selectComponent
                .AddOption("End current opening", "EndOpening")
                .AddOption("Extend current opening", "Extend");
        else if (isClosed)
            selectComponent
                .AddOption("End current closure", "EndClosure")
                .AddOption("Open now", "Now");
        else
            selectComponent
                .AddOption("Open now", "Now");
                
        selectComponent
            .AddOption("Open later", "Later")
            .AddOption("Cancel future opening", "CancelOpening");
            
        if (hasFutureClosure)
            selectComponent.AddOption("Cancel later closure", "CancelClosure");
        
        return new ComponentBuilder().WithSelectMenu(selectComponent).WithBackButton(c);
    }

    private Task OnSelect(ComponentVeniInteractionContext c)
    {
        c.Session.ClearItem(SessionKeys.OPENING_DATE);
        c.Session.ClearItem(SessionKeys.OPENING_HOUR);
        var selection = c.Interaction.Data.Values.Single();
        return selection switch
        {
            "EndOpening" => c.Session.MoveStateAsync<EndCurrentOpeningState>(c),
            "EndClosure" => c.Session.MoveStateAsync<EndCurrentClosureState>(c),
            "CancelOpening" => c.Session.MoveStateAsync<CancelOpeningState>(c),
            "CancelClosure" => c.Session.MoveStateAsync<CancelClosureState>(c),
            "Extend" => c.Session.MoveStateAsync<OpenHowLongWhenEntryState>(c),
            "Now" => c.Session.MoveStateAsync<OpenHowLongWhenEntryState>(c),
            "Later" => c.Session.MoveStateAsync<OpenTimeZoneEntryState>(c),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
}

