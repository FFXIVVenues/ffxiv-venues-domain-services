using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.TagsEntry;

class SceneEntrySessionState : ISessionState
{

    private Venue _venue;
    private static List<(string Label, string Value, string Emote)> _availableScenes = new()
    {
        (VenueControlStrings.SceneLabel_Nightclub, "Nightclub", "💃"),
        (VenueControlStrings.SceneLabel_Den, "Den", "🚬"),
        (VenueControlStrings.SceneLabel_Cafe, "Cafe", "☕"),
        (VenueControlStrings.SceneLabel_Tavern, "Tavern", "🍺"),
        (VenueControlStrings.SceneLabel_Inn, "Inn", "🛌"),
        (VenueControlStrings.SceneLabel_Lounge, "Lounge", "🍸"),
        (VenueControlStrings.SceneLabel_BathHouse, "Bath house", "🛁"),
        (VenueControlStrings.SceneLabel_Restaurant, "Restaurant", "🍴"),
        (VenueControlStrings.SceneLabel_Fightclub, "Fightclub", "🥊"),
        (VenueControlStrings.SceneLabel_Casino, "Casino", "🎰"),
        (VenueControlStrings.SceneLabel_Shop, "Shop", "🛍️"),
        (VenueControlStrings.SceneLabel_MaidCafe, "Maid cafe", "👔"),
        (VenueControlStrings.SceneLabel_Other, "Other", "❓")
    };

    public Task Enter(VeniInteractionContext c)
    {
        this._venue = c.Session.GetVenue(); 

        var component = this.BuildTagsComponent(c).WithBackButton(c).WithSkipButton<FeaturesEntrySessionState, FeaturesEntrySessionState>(c);
        return c.Interaction.RespondAsync(VenueControlStrings.AskForScenesMessage, component.Build());
    }

    private ComponentBuilder BuildTagsComponent(VeniInteractionContext c)
    {
        var selectComponent = new SelectMenuBuilder()
            .WithCustomId(c.Session.RegisterComponentHandler(OnComplete, ComponentPersistence.ClearRow))
            .WithMaxValues(Math.Max(2, _availableScenes.Count(t => this._venue.Tags?.Contains(t.Value) ?? false)));
        foreach (var (label, value, emote) in _availableScenes)
            selectComponent.AddOption(label, value, isDefault: this._venue.Tags.Contains(value), emote: new Emoji(emote));

        return new ComponentBuilder()
            .WithSelectMenu(selectComponent);
    }

    private Task OnComplete(ComponentVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        venue.Tags ??= new();
        venue.Tags.RemoveAll(existingTag => _availableScenes.Any(availableTag => existingTag == availableTag.Value));
        venue.Tags.AddRange(c.Interaction.Data.Values);

        return c.Session.MoveStateAsync<FeaturesEntrySessionState>(c);
    }

}