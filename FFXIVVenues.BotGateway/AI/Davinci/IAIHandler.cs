using System.Threading.Tasks;
using FFXIVVenues.BotGateway.Infrastructure.Context;

namespace FFXIVVenues.BotGateway.AI.Davinci
{
    internal interface IAIHandler
    {
        Task<string> ResponseHandler(MessageVeniInteractionContext context);
    }
}