using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Utils;
using Kana.Pipelines;

namespace FFXIVVenues.Veni.Infrastructure.Middleware
{
    class StopCallingMeMommyMiddleware : IMiddleware<MessageVeniInteractionContext>
    {

        private static readonly Regex _match = new ("\\bm+o+m+y+\\b", RegexOptions.IgnoreCase);

        private static readonly string[] _responses = new[]
        {
            "Please don't call me mommy. :facepalm:",
            "Staawp! I'm nobody's mommy! 😆",
            "Mommy? 😑"
        };

        public async Task ExecuteAsync(MessageVeniInteractionContext context, Func<Task> next)
        {
            var match = _match.Match(context.Interaction.Content);
            if (match.Success)
                await context.Interaction.Channel.SendMessageAsync(_responses.PickRandom());

            await next();
        }

    }
}
