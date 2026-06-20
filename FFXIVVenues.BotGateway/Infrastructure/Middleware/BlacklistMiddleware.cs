using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using FFXIVVenues.BotGateway.Authorisation.Blacklist;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Persistence.Abstraction;
using Kana.Pipelines;

namespace FFXIVVenues.BotGateway.Infrastructure.Middleware
{
    class BlacklistMiddleware : IMiddleware<MessageVeniInteractionContext>
    {
        private readonly IRepository _repository;

        public BlacklistMiddleware(IRepository repository)
        {
            this._repository = repository;
        }

        public async Task ExecuteAsync(MessageVeniInteractionContext context, Func<Task> next)
        {
            if (await this._repository.ExistsAsync<BlacklistEntry>(context.Interaction.Author.Id.ToString()))
            {
                var dm = await context.Client.GetUser(context.Interaction.Author.Id).CreateDMChannelAsync();
                await dm.SendMessageAsync($"Sorry, my family said I'm not allowed to speak to you. 😢" +
                                          $" If you think this was a mistake please let my family know.");
                return;
            }

            await next();
        }

    }
}
