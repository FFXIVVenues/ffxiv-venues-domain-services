using System;
using System.Threading.Tasks;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Intent;
using Kana.Pipelines;

namespace FFXIVVenues.BotGateway.Infrastructure.Middleware
{
    class IntentMiddleware : IMiddleware<MessageVeniInteractionContext>
    {
        private readonly IIntentHandlerProvider intentHandlerProvider;

        public IntentMiddleware(IIntentHandlerProvider intentHandlerProvider)
        {
            this.intentHandlerProvider = intentHandlerProvider;
        }

        public Task ExecuteAsync(MessageVeniInteractionContext context, Func<Task> next)
        {
            if (context.Prediction.TopIntent == IntentNames.None) 
                return next();
            
            return intentHandlerProvider.HandleIntent(context.Prediction.TopIntent, context);
        }

    }
}
