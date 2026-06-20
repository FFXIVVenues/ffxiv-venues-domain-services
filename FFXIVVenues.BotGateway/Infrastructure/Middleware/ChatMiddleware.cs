using Kana.Pipelines;
using System;
using System.Threading.Tasks;
using Serilog;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Utils;
using FFXIVVenues.BotGateway.AI.Davinci;

namespace FFXIVVenues.BotGateway.Infrastructure.Middleware
{
    internal class ChatMiddleware : IMiddleware<MessageVeniInteractionContext>
    {
        private readonly IAIHandler aIHandler;
        

        private static string[] _emotes = new[]
        {
            " ",
            " :3",
            " 💕",
            " 💖",
            " ❤️",
            " 💜",
            " 💞"
        };

        public ChatMiddleware(IAIHandler aIHandler)
        {
            this.aIHandler = aIHandler;
        }
        
        public async Task ExecuteAsync(MessageVeniInteractionContext context, Func<Task> next)
        {
            try
            {
                string response = await this.aIHandler.ResponseHandler(context);
                await context.Interaction.Channel.SendMessageAsync(response + _emotes.PickRandom());
            }
            catch (Exception ex)
            {
                Log.Warning(ex.Message, ex);
                await next();
            }

        }

    }
}
