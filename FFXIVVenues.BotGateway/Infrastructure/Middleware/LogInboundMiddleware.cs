using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using Kana.Pipelines;
using Serilog;


namespace FFXIVVenues.Veni.Infrastructure.Middleware
{
    internal class LogInboundMiddleware : IMiddleware<MessageVeniInteractionContext>
    {

        public Task ExecuteAsync(MessageVeniInteractionContext context, Func<Task> next)
        {
            ISessionState currentSessionState = null;
            context.Session.StateStack?.TryPeek(out currentSessionState);

            if (currentSessionState is not null) Log.Information("[{State}] {Username}: " + context.Interaction.Content, currentSessionState.GetType().Name, context.Interaction.Author.Username);
            else Log.Information("{Username}: " + context.Interaction.Content, context.Interaction.Author.Username);;
            
            return next();
        }
    }
}
