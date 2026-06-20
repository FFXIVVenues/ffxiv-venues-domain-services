using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;

namespace FFXIVVenues.Veni.AI.Davinci
{
    internal class AIHandler : IAIHandler
    {
        private readonly IDavinciService davinciService;
        private readonly IAIContextBuilder aIContextBuilder;

        public AIHandler(IDavinciService davinciService, IAIContextBuilder aIContextBuilder)
        {
            this.davinciService = davinciService;
            this.aIContextBuilder = aIContextBuilder;
        }

        public Task<string> ResponseHandler(MessageVeniInteractionContext context)
        {
            var messageContent = context.Interaction.Content;
            var id = context.Interaction.Author.Id.ToString();

            return this.davinciService.AskTheAI(this.aIContextBuilder.GetContext(id, messageContent));
        }

    }
}
