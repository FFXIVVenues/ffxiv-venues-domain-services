using System.Threading.Tasks;
using FFXIVVenues.Veni.AI.Clu.CluModels;

namespace FFXIVVenues.Veni.AI.Luis
{
    internal interface ICluClient
    {
        Task<CluPrediction> AnalyzeAsync(string query, ulong userId);
    }
}