using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using Kana.Pipelines;
using Serilog;


namespace FFXIVVenues.Veni.Infrastructure.Middleware
{
    internal class LogOutboundMiddleware : IMiddleware<MessageVeniInteractionContext>
    {

        public Task ExecuteAsync(MessageVeniInteractionContext context, Func<Task> next)
        {
            if (context.Interaction.Author.Id != context.Client.CurrentUser.Id)
                return next();

            ISessionState currentSessionState = null;
            context.Session.StateStack?.TryPeek(out currentSessionState);
            
            if (currentSessionState is not null) Log.Information("[{State}] {Username}: " + context.Interaction.Content + " ({EmbedCount} embeds) ({ComponentCount} components)", currentSessionState, context.Client.CurrentUser.Username, context.Interaction.Embeds.Count, context.Interaction.Components.Count);
            else Log.Information("{Username}: " + context.Interaction.Content + " ({EmbedCount} embeds) ({ComponentCount} components)", context.Client.CurrentUser.Username, context.Interaction.Embeds.Count, context.Interaction.Components.Count);
            
            return next();
        }
    }
}
