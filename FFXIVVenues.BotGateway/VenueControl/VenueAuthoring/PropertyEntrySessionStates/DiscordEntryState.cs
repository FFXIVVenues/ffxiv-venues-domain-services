using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation.Blacklist;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.ScheduleEntry;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates
{
    class DiscordEntrySessionState(IRepository db, IDiscordValidator discordValidator) : ISessionState
    {
        public Task Enter(VeniInteractionContext c)
        {
            c.Session.RegisterMessageHandler(this.OnMessageReceived);
            var isDm = c.Interaction.Channel is IDMChannel;
            return c.Interaction.RespondAsync(MessageRepository.AskForDiscordMessage.PickRandom(),
                new ComponentBuilder()
                    .WithBackButton(c)
                    .WithButton("No discord", c.Session.RegisterComponentHandler(OnNoDiscord, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .Build(),
                isDm ? null : new EmbedBuilder()
                    .WithDescription("**@ Veni Ki** with your discord link")
                    .WithColor(Color.Blue)
                    .Build());
        }

        private async Task OnMessageReceived(MessageVeniInteractionContext c)
        {
            var venue = c.Session.GetVenue();
            var rawDiscordString = c.Interaction.Content.StripMentions();

            if (!new Regex("^https?://").IsMatch(rawDiscordString))
                rawDiscordString = "https://" + rawDiscordString;

            var (discordValidity, invite) = await discordValidator.CheckInviteAsync(new Uri(rawDiscordString));
            switch (discordValidity)
            {
                case DiscordCheckResult.BadFormat:
                    await c.Interaction.Channel.SendMessageAsync("That doesn't look like a valid Discord invite to me. :thinking:");
                    return;
                case DiscordCheckResult.InvalidInvite:
                    await c.Interaction.Channel.SendMessageAsync("I tried that invite link but it seems to be invalid. :cry:");
                    return;
                case DiscordCheckResult.Expires:
                    await c.Interaction.Channel.SendMessageAsync(
                        $"I'm sorry, that invite link is not permanent, it'll expire on {invite.ExpiresAt.Value:m}. 🥲");
                    return;
            }

            if (await db.ExistsAsync<BlacklistEntry>(invite.GuildId.ToString()))
            {
                await c.Interaction.Channel.SendMessageAsync("This Discord server was blacklisted, please make a ticket in the FFXIV Venues discord for further information. 😢");
                return;
            }

            venue.Discord = new Uri(rawDiscordString);

            if (c.Session.InEditing())
            {
                await c.Session.MoveStateAsync<ConfirmVenueSessionState>(c);
                return;
            }
            await c.Session.MoveStateAsync<HaveScheduleEntrySessionState>(c);
        }
        
        private Task OnNoDiscord(ComponentVeniInteractionContext context)
        {
            var venue = context.Session.GetVenue();
            venue.Discord = null;

            if (context.Session.InEditing()) return context.Session.MoveStateAsync<ConfirmVenueSessionState>(context);
            return context.Session.MoveStateAsync<HaveScheduleEntrySessionState>(context);
        }
    }

}
