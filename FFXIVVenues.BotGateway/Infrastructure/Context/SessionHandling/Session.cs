using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Components;
using Microsoft.Extensions.DependencyInjection;
using Serilog;


namespace FFXIVVenues.Veni.Infrastructure.Context.SessionHandling
{
    public class Session
    {

        public Stack<ISessionState> StateStack { get; private set; } = new();
        public ConcurrentDictionary<string, object> Data { get; } = new();


        private readonly IServiceProvider _serviceProvider;
        private ConcurrentDictionary<string, ComponentSessionHandlerRegistration> _componentHandlers = new();
        private int _backClearance = 2;

        internal void SetBackClearanceAmount(int amount) =>
            this._backClearance = amount;

        private ConcurrentDictionary<string, Func<MessageVeniInteractionContext, Task>> _messageHandlers = new();

        public Session(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // todo: Add ability to provide constructor parameters so arguments can be passed and localised to the next state
        // This would drastically reduce the mental complexity and handling session items to communicate between states in
        // the time entry, biweekly, and monthly entry states, i.e. passing day forward.
        public async Task MoveStateAsync<T>(VeniInteractionContext context) where T : ISessionState
        {
            if (StateStack.TryPeek(out var currentState))
                Log.Debug("Set state from [{PreviousState}] to {State}", currentState?.GetType().Name, typeof(T).Name);
            else
                Log.Debug("Set state to [{State}]", typeof(T).Name);

            await this.ClearComponentHandlers(context);
            this.ClearMessageHandlers();
            var newState = ActivatorUtilities.CreateInstance<T>(_serviceProvider);
            StateStack.Push(newState);
            await newState.Enter(context);
        }

        public Task MoveStateAsync<T>(IWrappableInteraction context) where T : ISessionState =>
            this.MoveStateAsync<T>(context.ToWrappedInteraction());

        public async Task<bool> TryBackStateAsync(VeniInteractionContext context)
        {
            if (!StateStack.TryPop(out var currentState))
                return false;
            if (!StateStack.TryPeek(out var newState))
                return false;
            Log.Debug("Back state from [{PreviousState}] to [{State}]", currentState?.GetType().Name, newState?.GetType().Name);
            await this.ClearComponentHandlers(context);
            this.ClearMessageHandlers();
            await newState.Enter(context);
            return true;
        }

        public Task<bool> TryBackStateAsync(IWrappableInteraction context) =>
            this.TryBackStateAsync(context.ToWrappedInteraction());

        public async Task ClearStateAsync(VeniInteractionContext context)
        {
            this.Data.Clear();
            await this.ClearComponentHandlers(context);
            this.ClearMessageHandlers();
            StateStack = new();
        }

        public Task ClearStateAsync(IWrappableInteraction context) =>
            this.ClearStateAsync(context.ToWrappedInteraction());

        // todo: Replace the session dictionary with ISession<T> where T is a type context object
        // this will allow long term storage through the session
        
        public T GetItem<T>(string name)
        {
            var itemFound = Data.TryGetValue(name, out var item);
            if (!itemFound) return default;
            return (T)item;
        }

        public void ClearItem(string name)
        {
            Data.TryRemove(name, out _);
        }

        public void SetItem<T>(string name, T item)
        {
            Data.AddOrUpdate(name, item, (s, o) => item);
        }

        public string RegisterComponentHandler(Func<ComponentVeniInteractionContext, Task> @delegate, ComponentPersistence persistence)
        {
            var key = Guid.NewGuid().ToString();
            var registration = new ComponentSessionHandlerRegistration(@delegate, persistence);
            this._componentHandlers[key] = registration;
            return key;
        }

        public void UnregisterComponentHandler(string key)
        {
            this._componentHandlers.TryRemove(key, out _);
        }

        public async Task ClearComponentHandlers(VeniInteractionContext context)
        {
            await this.ClearPreviousComponents(context.Interaction.Channel, context.Client.CurrentUser.Id);
            this._componentHandlers.Clear();
        }

        public Task HandleComponentInteraction(ComponentVeniInteractionContext context)
        {
            var key = context.Interaction.Data.CustomId.Split(":");
            if (key[0] == ComponentBroker.ValuesToHandlersKey)
                key = context.Interaction.Data.Values?.FirstOrDefault()?.Split(":");
            
            if (!this._componentHandlers.TryGetValue(key[0], out var handler))
                return Task.CompletedTask;

            if (handler.Persistence == ComponentPersistence.ClearRow)
                _ = context.Interaction.ModifyOriginalResponseAsync(props => props.Components = new ComponentBuilder().Build());
            if (handler.Persistence == ComponentPersistence.DeleteMessage)
                _ = context.Interaction.DeleteOriginalResponseAsync();

            return handler.Delegate(context);
        }

        public string RegisterMessageHandler(Func<MessageVeniInteractionContext, Task> @delegate)
        {
            var key = Guid.NewGuid().ToString();
            this._messageHandlers[key] = @delegate;
            return key;
        }

        public void UnregisterMessageHandler(string key)
        {
            this._messageHandlers.TryRemove(key, out _);
        }


        public void ClearMessageHandlers()
        {
            this._messageHandlers.Clear();
        }

        public async Task<bool> HandleMessageAsync(MessageVeniInteractionContext context)
        {
            var handled = false;
            foreach (var handler in this._messageHandlers)
            {
                await handler.Value(context);
                handled = true;
            }

            return handled;
        }

        // TODO: Move this to ClearComponentHandlers method
        private async Task ClearPreviousComponents(ISocketMessageChannel channel, ulong currentUser)
        {
            await foreach (var messages in channel.GetMessagesAsync(this._backClearance))
            foreach (var message in messages)
            {
                if (message.Author.Id == currentUser && message.Components.Any())
                foreach (var wrappingComponent in message.Components)
                {
                    if (!(wrappingComponent is ActionRowComponent actionRow))
                        continue;

                    var rowCleared = false;
                    var messageDeleted = false;
                    foreach (var component in actionRow.Components)
                        if (this._componentHandlers.TryGetValue(component.CustomId, out var handler))
                            if (!rowCleared && handler.Persistence == ComponentPersistence.ClearRow)
                            {
                                _ = channel.ModifyMessageAsync(message.Id, m => m.Components = new ComponentBuilder().Build());
                                rowCleared = true;
                                break;
                            }
                            else if (handler.Persistence == ComponentPersistence.DeleteMessage)
                            {
                                _ = channel.DeleteMessageAsync(message.Id);
                                messageDeleted = true;  
                                break;
                            }

                    if (messageDeleted)
                        break;
                }
            }
        }
    }
}
