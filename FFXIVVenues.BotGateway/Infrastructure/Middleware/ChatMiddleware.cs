using FFXIVVenues.Veni.Utils;
using Kana.Pipelines;
using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.AI.Davinci;
using FFXIVVenues.Veni.Infrastructure.Context;
using Serilog;

namespace FFXIVVenues.Veni.Infrastructure.Middleware
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
