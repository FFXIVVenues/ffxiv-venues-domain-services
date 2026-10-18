using Discord;
using FFXIVVenues.BotGateway.Infrastructure.Components;
using FFXIVVenues.BotGateway.VenueEvents.VenueFlags.Responses;
using FFXIVVenues.BotGateway.VenueRendering;
using FFXIVVenues.DomainData.Entities.Flags;
using FFXIVVenues.VenueModels;
using System;

namespace FFXIVVenues.BotGateway.VenueEvents.VenueFlags;

public interface IVenueFlagRenderer {
    EmbedBuilder RenderFlag(Venue venue, Flag flag);
    ComponentBuilder RenderFlagOptions(Flag flag, RenderOptions options = RenderOptions.All);
}

public class VenueFlagRenderer(UiConfiguration uiConfig) : IVenueFlagRenderer
{
    public EmbedBuilder RenderFlag(Venue venue, Flag flag)
    {
        var embed = new EmbedBuilder()
            .WithAuthor(flag.Category switch {
                    FlagCategory.VenueEmpty => FlagStrings.VenueFlaggedVenueEmpty,
                    FlagCategory.InappropriateContent => FlagStrings.VenueFlaggedInappropriateContent,
                    FlagCategory.IncorrectInformation => FlagStrings.VenueFlaggedInfoIncorrect,
                    _ => flag.Category.ToString() })
            .WithTitle(venue.Name)
            .WithTimestamp(flag.Timestamp)
            .WithUrl(uiConfig.BaseUrl + "/venue/" + venue.Id)
            .WithDescription(flag.Description)
            .WithFooter(flag.SourceAddress.Substring(0, 10))
            .WithColor(Color.Red);
        return embed;
    }

    public ComponentBuilder RenderFlagOptions(Flag flag, RenderOptions options = RenderOptions.All)
    {
        var builder = new ComponentBuilder();
        var dropDown = new SelectMenuBuilder()
            .WithValueHandlers()
            .WithPlaceholder(FlagStrings.SelectResponse);

        if (options.HasFlag(RenderOptions.Forward))
            dropDown.AddOption(new SelectMenuOptionBuilder()
                .WithLabel(FlagStrings.ForwardFlag)
                .WithEmote(new Emoji("⏩"))
                .WithDescription(FlagStrings.ForwardFlagDescription)
                .WithStaticHandler(ForwardFlagHandler.Key, flag.Id));

        if (options.HasFlag(RenderOptions.Resolve))
            dropDown.AddOption(new SelectMenuOptionBuilder()
                .WithLabel(FlagStrings.ResolveFlag)
                .WithEmote(new Emoji("✅"))
                .WithDescription(FlagStrings.ResolveFlagDescription)
                .WithStaticHandler(ResolveFlagHandler.Key, flag.Id));

        if (options.HasFlag(RenderOptions.Dismiss))
            dropDown.AddOption(new SelectMenuOptionBuilder()
                .WithLabel(FlagStrings.DismissFlag)
                .WithEmote(new Emoji("❎"))
                .WithDescription(FlagStrings.DismissFlagDescription)
                .WithStaticHandler(DismissFlagHandler.Key, flag.Id));

        builder.WithSelectMenu(dropDown);
        return builder;
    }

}

[Flags]
public enum RenderOptions
{
    None = 0,
    Forward = 1,
    Resolve = 2, 
    Dismiss = 4,
    ResolveDismiss = Resolve | Dismiss,
    All = Forward | Resolve | Dismiss
}