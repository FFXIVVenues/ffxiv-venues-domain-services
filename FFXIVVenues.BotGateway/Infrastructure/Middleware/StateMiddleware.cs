using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using Kana.Pipelines;

namespace FFXIVVenues.Veni.Infrastructure.Middleware
{
    class StateMiddleware : IMiddleware<MessageVeniInteractionContext>
    {

        public async Task ExecuteAsync(MessageVeniInteractionContext context, Func<Task> next)
        {
            if (context.Session.StateStack == null)
            {
                await next();
                return;
            }
            
            var handled = await context.Session.HandleMessageAsync(context);
            if (!handled) await next();
        }

    }
}
