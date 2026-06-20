using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.TagsEntry;

class FeaturesEntrySessionState : ISessionState
{
    private Venue _venue;

    private static List<(string Label, string Description, string Value, string Emote)> _availableTags = new()
    {
        (VenueControlStrings.TagLabel_Courtesans, VenueControlStrings.TagDescription_Courtesans, "Courtesans", "🔞"),
        (VenueControlStrings.TagLabel_Gambling, VenueControlStrings.TagDescription_Gambling, "Gambling", "🎲"),
        (VenueControlStrings.TagLabel_Artists, VenueControlStrings.TagDescription_Artists, "Artists", "🎨"),
        (VenueControlStrings.TagLabel_Dancers, VenueControlStrings.TagDescription_Dancers, "Dancers", "💃"),
        (VenueControlStrings.TagLabel_Bards, VenueControlStrings.TagDescription_Bards, "Bards", "🎵"),
        (VenueControlStrings.TagLabel_Food, VenueControlStrings.TagDescription_Food, "Food", "🍲"),
        (VenueControlStrings.TagLabel_Drink, VenueControlStrings.TagDescription_Drink, "Drink", "🍷"),
        (VenueControlStrings.TagLabel_TwitchDJ, VenueControlStrings.TagDescription_TwitchDJ, "Twitch DJ", "🎧"),
        (VenueControlStrings.TagLabel_SyncDJ, VenueControlStrings.TagDescription_SyncDJ, "Sync DJ", "🎧"),
        (VenueControlStrings.TagLabel_Bar, VenueControlStrings.TagDescription_Bar, "Bar", "🍺"),
        (VenueControlStrings.TagLabel_Tarot, VenueControlStrings.TagDescription_Tarot, "Tarot", "🔮"),
        (VenueControlStrings.TagLabel_PillowTalk, VenueControlStrings.TagDescription_PillowTalk, "Pillow", "💬"),
        (VenueControlStrings.TagLabel_Photography, VenueControlStrings.TagDescription_Photography, "Photography", "📷"),
        (VenueControlStrings.TagLabel_OpenStage, VenueControlStrings.TagDescription_OpenStage, "Open stage", "🎤"),
        (VenueControlStrings.TagLabel_Void, VenueControlStrings.TagDescription_Void, "Void", "🌒"),
        (VenueControlStrings.TagLabel_Stylists, VenueControlStrings.TagDescription_Stylists, "Stylists", "💇"),
        (VenueControlStrings.TagLabel_NovelPerformances, VenueControlStrings.TagDescription_NovelPerformances, "Performances", "💃"),
        (VenueControlStrings.TagLabel_Giveaways, VenueControlStrings.TagDescription_Giveaways, "Giveaways", "🎁"),
        (VenueControlStrings.TagLabel_SyncshellAvailable, VenueControlStrings.TagDescription_SyncshellAvailable, "Syncshell available", "👀"),
        (VenueControlStrings.TagLabel_VIPAvailable, VenueControlStrings.TagDescription_VIPAvailable, "VIP", "💎"),
        (VenueControlStrings.TagLabel_LgbtqiaFocused, VenueControlStrings.TagDescription_LgbtqiaFocused, "LGBTQIA+", "🏳️‍🌈"),
        (VenueControlStrings.TagLabel_IcRpEncouraged, VenueControlStrings.TagDescription_IcRpEncouraged, "RP Heavy", "🎭"),
        (VenueControlStrings.TagLabel_IcRpOnly, VenueControlStrings.TagDescription_IcRpOnly, "IC RP Only", "🎭"),
        (VenueControlStrings.TagLabel_OpenHouse247, VenueControlStrings.TagDescription_OpenHouse247, "24/7 open house", "🏠")
    };

    public Task Enter(VeniInteractionContext c)
    {
        this._venue = c.Session.GetVenue();

        var component = this.BuildTagsComponent(c);
        return c.Interaction.RespondAsync(VenueControlStrings.AskForFeaturesMessage, component.Build());
    }

    private ComponentBuilder BuildTagsComponent(VeniInteractionContext c)
    {
        var selectComponent = new SelectMenuBuilder()
            .WithCustomId(c.Session.RegisterComponentHandler(OnComplete, ComponentPersistence.ClearRow))
            .WithMaxValues(_availableTags.Count);
        foreach (var (label, desc, value, emote) in _availableTags)
            selectComponent.AddOption(label, value, isDefault: this._venue.Tags.Contains(value), description: desc, emote: new Emoji(emote));

        return new ComponentBuilder()
            .WithSelectMenu(selectComponent)
            .WithBackButton(c)
            .WithSkipButton<GamesEntrySessionState, ConfirmVenueSessionState>(c);
    }

    private Task OnComplete(ComponentVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        venue.Tags ??= new();
        venue.Tags.RemoveAll(existingTag => _availableTags.Any(availableTag => existingTag == availableTag.Value));
        venue.Tags.AddRange(c.Interaction.Data.Values);

        return c.Session.MoveStateAsync<GamesEntrySessionState>(c);
    }

}