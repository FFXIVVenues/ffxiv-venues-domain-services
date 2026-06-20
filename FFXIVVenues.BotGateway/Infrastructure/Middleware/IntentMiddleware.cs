using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using Kana.Pipelines;

namespace FFXIVVenues.Veni.Infrastructure.Middleware
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
