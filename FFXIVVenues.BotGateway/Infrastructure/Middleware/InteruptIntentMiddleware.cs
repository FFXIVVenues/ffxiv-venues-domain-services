using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using Kana.Pipelines;

namespace FFXIVVenues.Veni.Infrastructure.Middleware
{
    class InteruptIntentMiddleware : IMiddleware<MessageVeniInteractionContext>
    {

        private readonly IIntentHandlerProvider intentHandlerProvider;

        public InteruptIntentMiddleware(IIntentHandlerProvider intentHandlerProvider)
        {
            this.intentHandlerProvider = intentHandlerProvider;
        }

        public Task ExecuteAsync(MessageVeniInteractionContext context, Func<Task> next)
        {
            var handleTask = intentHandlerProvider.HandleIteruptIntent(context.Prediction.TopIntent, context);
            if (handleTask == null)
                return next();

            return handleTask;
        }

    }
}
