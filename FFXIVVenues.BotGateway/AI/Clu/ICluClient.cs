using System.Threading.Tasks;
using FFXIVVenues.BotGateway.AI.Clu.CluModels;

namespace FFXIVVenues.BotGateway.AI.Clu
{
    internal interface ICluClient
    {
        Task<CluPrediction> AnalyzeAsync(string query, ulong userId);
    }
}