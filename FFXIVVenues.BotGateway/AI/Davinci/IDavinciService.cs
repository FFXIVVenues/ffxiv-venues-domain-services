using System.Threading.Tasks;

namespace FFXIVVenues.Veni.AI.Davinci
{
    internal interface IDavinciService
    {
        Task<string> AskTheAI(string prompt);

    }
}