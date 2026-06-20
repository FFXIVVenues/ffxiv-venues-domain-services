using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates
{
    class WebsiteEntrySessionState : ISessionState
    {
        public Task Enter(VeniInteractionContext c)
        {
            c.Session.RegisterMessageHandler(this.OnMessageReceived);
            var isDm = c.Interaction.Channel is IDMChannel;

            return c.Interaction.RespondAsync(MessageRepository.AskForWebsiteMessage.PickRandom(),
                new ComponentBuilder()
                    .WithBackButton(c)
                    .WithButton("No website", c.Session.RegisterComponentHandler(OnNoWebsite, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .Build(),
                isDm ? null : new EmbedBuilder()
                    .WithDescription("**@ Veni Ki** with your web link")
                    .WithColor(Color.Blue)
                    .Build());
        }

        private Task OnMessageReceived(MessageVeniInteractionContext c)
        {
            var venue = c.Session.GetVenue();
            if (new Regex("\\bskip\\b").IsMatch(c.Interaction.Content.ToLower()))
            {
                if (c.Session.InEditing())
                    return c.Session.MoveStateAsync<ConfirmVenueSessionState>(c);
                return c.Session.MoveStateAsync<DiscordEntrySessionState>(c);
            }

            var rawWebsiteString = c.Interaction.Content.StripMentions();
            if (!new Regex("^https?://").IsMatch(rawWebsiteString))
                rawWebsiteString = "https://" + rawWebsiteString;

            if (!Uri.TryCreate(rawWebsiteString, UriKind.Absolute, out var website))
            {
                c.Interaction.Channel.SendMessageAsync("Sorry, that doesn't look like a valid website address.");
                return Task.CompletedTask;
            }

            venue.Website = website;

            if (c.Session.InEditing())
                return c.Session.MoveStateAsync<ConfirmVenueSessionState>(c);

            return c.Session.MoveStateAsync<DiscordEntrySessionState>(c);
        }
        
        private Task OnNoWebsite(ComponentVeniInteractionContext context)
        {
            var venue = context.Session.GetVenue();
            venue.Website = null;

            if (context.Session.InEditing()) return context.Session.MoveStateAsync<ConfirmVenueSessionState>(context);
            return context.Session.MoveStateAsync<DiscordEntrySessionState>(context);
        }
    }
}
