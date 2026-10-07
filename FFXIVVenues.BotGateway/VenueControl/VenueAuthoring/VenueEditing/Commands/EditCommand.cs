using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Infrastructure.Commands;
using FFXIVVenues.BotGateway.Infrastructure.Commands.Attributes;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.VenueEditing.ComponentHandlers;
using FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.VenueEditing.SessionStates;
using FFXIVVenues.BotGateway.VenueRendering;

namespace FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.VenueEditing.Commands;

[DiscordCommand("edit", "Edit your venue(s)!")]
internal class EditCommand(IApiService apiService, DtoVenueRenderer venueRenderer) : ICommandHandler
{
    public async Task HandleAsync(SlashCommandVeniInteractionContext context)
    {
        var user = context.Interaction.User.Id;
        var venues = await apiService.GetAllVenuesAsync(user);

        if (venues == null || !venues.Any())
        {
            await context.Interaction.RespondAsync("You don't seem to be an assigned manager for any venues. 🤔");
            return;
        }

        await context.Session.ClearStateAsync(context);
            
        // ReSharper disable once PossibleMultipleEnumeration
        // Enumerating next once for the Any is better than enumerating all on a chance
        venues = venues.ToList();
        if (venues.Count() == 1)
        {
            var venue = venues.Single();
            context.Session.SetVenue(venue);
            await context.Session.MoveStateAsync<EditVenueSessionState>(context);
            return;
        }
            
        if (venues.Count() > 25)
            venues = venues.Take(25);
        await context.Interaction.RespondAsync(VenueControlStrings.SelectVenueToEdit,
            components: venueRenderer.RenderVenueSelection(venues, SelectVenueToEditHandler.Key).Build());
    }

}
