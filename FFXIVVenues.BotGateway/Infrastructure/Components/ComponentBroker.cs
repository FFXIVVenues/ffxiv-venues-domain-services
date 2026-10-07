using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Utils;

namespace FFXIVVenues.BotGateway.Infrastructure.Components;

public class ComponentBroker(IServiceProvider serviceProvider) : IComponentBroker
{
    
    public const string ValuesToHandlersKey = "VALUES_TO_HANDLERS";
    private readonly TypeMap<IComponentHandler> _handlers = new(serviceProvider);

    public void Add<THandler>(string key) where THandler : IComponentHandler =>
        this._handlers.Add<THandler>(key);

    public Task HandleAsync(ComponentVeniInteractionContext context)
    {
        var key = context.Interaction.Data.CustomId.Split(":");
        if (key[0] == ValuesToHandlersKey)
            key = context.Interaction.Data.Values?.FirstOrDefault()?.Split(":");
        var handler = this._handlers.Activate(key[0], ActivateResolve.NewServiceScope);
        if (handler == default)
            return Task.CompletedTask;
        return handler.HandleAsync(context, key[1].Split(','));
    }

}
