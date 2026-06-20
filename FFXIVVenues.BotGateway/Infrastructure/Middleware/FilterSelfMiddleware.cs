using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Context;
using Kana.Pipelines;

namespace FFXIVVenues.Veni.Infrastructure.Middleware
{
    class FilterSelfMiddleware : IMiddleware<MessageVeniInteractionContext>
    {

        public Task ExecuteAsync(MessageVeniInteractionContext context, Func<Task> next)
        {
            if (context.Interaction.Author.Id != context.Client.CurrentUser.Id)
                return next();
            return Task.CompletedTask;
        }

    }
}
