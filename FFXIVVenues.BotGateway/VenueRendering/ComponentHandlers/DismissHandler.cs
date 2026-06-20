using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.BotGateway.Infrastructure.Components;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.VenueControl;

namespace FFXIVVenues.BotGateway.VenueRendering.ComponentHandlers;

public class DismissHandler : IComponentHandler
{

    // Change this key and any existing buttons linked to this will die
    public static string Key => "CONTROL_DISMISS";

    public async Task HandleAsync(ComponentVeniInteractionContext context, string[] args)
    {
        await context.Interaction.ModifyOriginalResponseAsync(props =>
            props.Components = new ComponentBuilder().Build());
        if (!await context.Session.TryBackStateAsync(context))
            await context.Interaction.Channel.SendMessageAsync(VenueControlStrings.Dismissed);
    }
         
}