using System.Threading.Tasks;

namespace FFXIVVenues.BotGateway.AI.Davinci
{
    internal interface IDavinciService
    {
        Task<string> AskTheAI(string prompt);

    }
}