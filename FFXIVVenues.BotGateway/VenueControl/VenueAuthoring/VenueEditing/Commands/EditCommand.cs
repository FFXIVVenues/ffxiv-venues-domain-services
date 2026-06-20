using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.BotGateway.Api;
using FFXIVVenues.BotGateway.Infrastructure.Commands;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.VenueControl;
using FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.VenueEditing.ComponentHandlers;
using FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.VenueEditing.SessionStates;
using FFXIVVenues.BotGateway.VenueRendering;
using FFXIVVenues.BotGateway.VenueAuditing.ComponentHandlers.AuditResponse;

namespace FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.VenueEditing.Commands
{
    public static class EditCommand
    {

        public const string COMMAND_NAME = "edit";

        internal class Factory : ICommandFactory
        {

            public SlashCommandProperties GetSlashCommand()
            {
                return new SlashCommandBuilder()
                    .WithName(COMMAND_NAME)
                    .WithDescription("Edit your venue(s)!")
                    .Build();
            }

        }

        internal class Handler(IApiService apiService, IVenueRenderer venueRenderer) : ICommandHandler
        {
            private readonly IApiService _apiService = apiService;
            private readonly IVenueRenderer _venueRenderer = venueRenderer;

            public async Task HandleAsync(SlashCommandVeniInteractionContext context)
            {
                var user = context.Interaction.User.Id;
                var venues = await this._apiService.GetAllVenuesAsync(user);

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
                    components: this._venueRenderer.RenderVenueSelection(venues, SelectVenueToEditHandler.Key).Build());
            }

        }

    }
}
