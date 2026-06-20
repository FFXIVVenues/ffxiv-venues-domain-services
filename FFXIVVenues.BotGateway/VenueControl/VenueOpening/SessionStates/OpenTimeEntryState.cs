using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Context.SessionHandling;
using FFXIVVenues.BotGateway.Utils;
using FFXIVVenues.BotGateway.VenueControl;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Authorisation;
using FFXIVVenues.VenueModels;
using SixLabors.ImageSharp.Formats.Tiff;

namespace FFXIVVenues.BotGateway.VenueControl.VenueOpening.SessionStates;

internal class OpenTimeEntryState : ISessionState
{
    public Task Enter(VeniInteractionContext c)
    {
        var component = this.BuildOpenComponent(c).WithBackButton(c);
        return c.Interaction.RespondAsync(VenueControlStrings.AskForTimeOfOpening, component.Build()); //change text later
    }

    private ComponentBuilder BuildOpenComponent(VeniInteractionContext c)
    {
        var selectComponent = new SelectMenuBuilder()
            .WithCustomId(c.Session.RegisterComponentHandler(OnSelect, ComponentPersistence.ClearRow));
        for (var i = 0; i < 24; i++)
            selectComponent.AddOption($"{i % 12}:00{(i > 12 ? "pm" : "am")}", i.ToString());
        return new ComponentBuilder().WithSelectMenu(selectComponent);
    }

    private Task OnSelect(ComponentVeniInteractionContext c)
    {
        var hourSelection = int.Parse(c.Interaction.Data.Values.Single()); 
        c.Session.SetItem(SessionKeys.OPENING_HOUR, hourSelection);
        return c.Session.MoveStateAsync<OpenHowLongWhenEntryState>(c);
    }
}
