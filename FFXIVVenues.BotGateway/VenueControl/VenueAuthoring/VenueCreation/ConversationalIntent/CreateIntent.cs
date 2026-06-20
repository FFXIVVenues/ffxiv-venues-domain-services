using System.Threading.Tasks;
using Discord;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Intent;
using FFXIVVenues.BotGateway.Utils;
using FFXIVVenues.BotGateway.VenueControl;
using FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.PropertyEntrySessionStates;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.VenueCreation.ConversationalIntent
{
    internal class CreateIntent : IntentHandler
    {

        private const string CREATE_VALUE_KEY = "venue";

        public override async Task Handle(VeniInteractionContext context)
        {
            context.Session.SetIsNewVenue();

            var venue = new Venue();
            venue.Managers.Add(context.Interaction.User.Id.ToString());
            context.Session.Data.AddOrUpdate(CREATE_VALUE_KEY, (s, v) => v, (s, e, v) => v, venue);
            if (context.Interaction.IsDM)
                await context.Interaction.RespondAsync(MessageRepository.CreateVenueMessage.PickRandom());
            else
                await context.Interaction.RespondAsync(MessageRepository.CreateVenueMessage.PickRandom(),
                    embed: new EmbedBuilder {
                        Color = Color.Red,
                        Description = MessageRepository.MentionOrReplyToMeMessage.PickRandom()
                    }.Build());
            await context.Session.MoveStateAsync<NameEntrySessionState>(context);
        }

    }
}
