using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.TagsEntry;

class GamesEntrySessionState : ISessionState
{
    private Venue _venue;

    private static List<(string Label, string Description, string Value, string Emote)> _availableGames = new()
    {
        (VenueControlStrings.TagLabel_TripleTriad, VenueControlStrings.TagDescription_TripleTriad, "Triple triad", "🎴"),
        (VenueControlStrings.TagLabel_TruthOrDare, VenueControlStrings.TagDescription_TruthOrDare, "Truth or dare", "❓"),
        (VenueControlStrings.TagLabel_Blackjack, VenueControlStrings.TagDescription_Blackjack, "Blackjack", "🃏"),
        (VenueControlStrings.TagLabel_Deathroll, VenueControlStrings.TagDescription_Deathroll, "Deathroll", "🎲"),
        (VenueControlStrings.TagLabel_TexasHoldem, VenueControlStrings.TagDescription_TexasHoldem, "Texas holdem", "♠️"),
        (VenueControlStrings.TagLabel_Bingo, VenueControlStrings.TagDescription_Bingo, "Bingo", "🔢"),
        (VenueControlStrings.TagLabel_Roulette, VenueControlStrings.TagDescription_Roulette, "Roulette", "🎡")
    };

    public Task Enter(VeniInteractionContext c)
    {
        this._venue = c.Session.GetVenue();

        var component = this.BuildTagsComponent(c);
        return c.Interaction.RespondAsync(VenueControlStrings.AskForGamesMessage, component.Build());
    }

    private ComponentBuilder BuildTagsComponent(VeniInteractionContext c)
    {
        var selectComponent = new SelectMenuBuilder()
            .WithCustomId(c.Session.RegisterComponentHandler(OnComplete, ComponentPersistence.ClearRow))
            .WithMaxValues(_availableGames.Count);
        foreach (var (label, description, value, emote) in _availableGames)
            selectComponent.AddOption(label, value, isDefault: this._venue.Tags.Contains(value), description: description, emote: new Emoji(emote));

        return new ComponentBuilder()
            .WithSelectMenu(selectComponent)
            .WithBackButton(c)
            .WithSkipButton<WebsiteEntrySessionState, ConfirmVenueSessionState>(c);
    }

    private Task OnComplete(ComponentVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        venue.Tags ??= new();
        venue.Tags.RemoveAll(existingTag => _availableGames.Any(availableTag => existingTag == availableTag.Value));
        venue.Tags.AddRange(c.Interaction.Data.Values);

        if (c.Session.InEditing())
            return c.Session.MoveStateAsync<ConfirmVenueSessionState>(c);

        return c.Session.MoveStateAsync<WebsiteEntrySessionState>(c);
    }

}