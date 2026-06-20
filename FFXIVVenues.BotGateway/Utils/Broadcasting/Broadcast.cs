using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace FFXIVVenues.Veni.Utils.Broadcasting
{
    public record Broadcast(string Id, IDiscordClient Client)
    {

        private List<BroadcastMessage> _broadcastedMessages;
        public string Message { get; private set; }
        public ComponentBuilder Component { get; private set; }
        public EmbedBuilder Embed { get; private set; }

        private ConcurrentDictionary<string, Func<BroadcastInteractionContext, Task>> _handlers = new();


        public Broadcast WithMessage(string message)
        {
            Message = message;
            return this;
        }

        public Broadcast WithComponent(Func<ComponentContext, ComponentBuilder> componentBuilder)
        {
            Component = componentBuilder(new ComponentContext(this));
            return this;
        }

        public Broadcast WithEmbed(EmbedBuilder embedBuilder)
        {
            Embed = embedBuilder;
            return this;
        }

        public async Task<BroadcastReceipt> SendToAsync(params ulong[] users)
        {
            this._broadcastedMessages = new ();
            foreach (var userId in users)
            {
                var broadcastedMessage = await SendTo(userId);
                this._broadcastedMessages.Add(broadcastedMessage);
            }

            return new (this.Id, _broadcastedMessages);
        }

        private async Task<BroadcastMessage> SendTo(ulong userId)
        {
            IUser user;
            IDMChannel channel;
            IUserMessage message;
            try
            {
                user = await Client.GetUserAsync(userId);
            }
            catch (Exception e)
            {
                return new(userId, null, MessageStatus.Failed,
                    $"Could not get user. {e.Message}");
            }
            
            if (user == null)
            {
                return new(userId, null, MessageStatus.Failed,
                    $"User was not found.");
            }
            
            if (user.Username.StartsWith("Deleted User"))
            {
                return new(userId, null, MessageStatus.FailedUserDeleted,
                    $"Skipped user. Username indicated user has been deleted.");
            }

            try
            {
                channel = await user.CreateDMChannelAsync();
            }
            catch (Exception e)
            {
                return new(userId, null, MessageStatus.Failed,
                    $"Could not create DM channel with user. {e.Message}");
            }

            try
            {
                message = await channel.SendMessageAsync(Message,
                    components: Component?.Build(),
                    embed: Embed?.Build());
            }
            catch (Exception e)
            {
                return new(userId, null, MessageStatus.Failed,
                    $"Could not send message to user. {e.Message}");
            }

            return new(userId, message, MessageStatus.Sent, "Broadcast message sent successfully");
        }

        public async Task<bool> HandleComponentInteraction(SocketMessageComponent c)
        {
            if (_handlers.ContainsKey(c.Data.CustomId))
            {
                var context = new BroadcastInteractionContext(this, c);
                await _handlers[c.Data.CustomId](context);
                return true;
            }
            return false;
        }

        public record ComponentContext(Broadcast Broadcast)
        {

            public string RegisterComponentHandler(Func<BroadcastInteractionContext, Task> handler)
            {
                var key = Guid.NewGuid().ToString();
                Broadcast._handlers[key] = handler;
                return key;
            }

        }

        public record BroadcastInteractionContext(Broadcast Broadcast, SocketMessageComponent Component)
        {

            public SocketUser CurrentUser => Component.User;

            public async Task ModifyForOtherUsers(Action<MessageProperties, IUserMessage> modifier)
            {
                var currentUser = Component.User.Id;
                foreach (var sentMessage in Broadcast._broadcastedMessages)
                {
                    if (sentMessage.UserId == currentUser)
                        continue;
                    await sentMessage.Message.ModifyAsync(props => modifier(props, sentMessage.Message));
                }
            }

            public async Task ModifyForCurrentUser(Action<MessageProperties, IUserMessage> modifier)
            {
                var currentUser = Component.User.Id;
                foreach (var sentMessage in Broadcast._broadcastedMessages)
                {
                    if (sentMessage.UserId != currentUser)
                        continue;
                    await sentMessage.Message.ModifyAsync(props => modifier(props, sentMessage.Message));
                }
            }

        }
    }


}

