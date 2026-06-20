using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueControl;
using FFXIVVenues.Veni.VenueDiscovery.SessionStates;
using FFXIVVenues.Veni.VenueRendering;

namespace FFXIVVenues.Veni.VenueDiscovery.Commands
{
    [DiscordCommand("find", "Find a venue by it's name.")]
    [DiscordCommandOption("query", "Part or all of the name of the venues you want to find", ApplicationCommandOptionType.String, Required = true)]
    public class FindCommand(IApiService apiService, IVenueRenderer venueRenderer) : ICommandHandler
    {
        public async Task HandleAsync(SlashCommandVeniInteractionContext context)
        {
            var asker = context.Interaction.User.Id;
            var query = context.GetStringArg("query");

            if (string.IsNullOrWhiteSpace(query))
            {
                await context.Interaction.RespondAsync("What am I looking for? 🤔");
                return;
            }

            var venues = await apiService.GetAllVenuesAsync(query);

            if (venues == null || !venues.Any())
                await context.Interaction.RespondAsync("Could find any venues with that name. 😔");
            else if (venues.Count() > 1)
            {
                if (venues.Count() > 25)
                    venues = venues.Take(25);
                context.Session.SetItem(SessionKeys.VENUES, venues);
                await context.Session.MoveStateAsync<SelectVenueToShowSessionState>(context);
            }
            else
            {
                var venue = venues.Single();
                await context.Interaction.RespondAsync(embed: (await venueRenderer.ValidateAndRenderAsync(venue)).Build(),
                    components: venueRenderer.RenderActionComponents(context, venue, asker).Build());
            }
        }

    }
}
