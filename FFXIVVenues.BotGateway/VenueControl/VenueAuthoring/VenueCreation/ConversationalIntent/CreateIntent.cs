using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueCreation.ConversationalIntent
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
