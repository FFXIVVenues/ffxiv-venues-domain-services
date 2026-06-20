using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.AI.Language.Conversations;
using Azure.Core;
using FFXIVVenues.Veni.AI.Clu.CluModels;
using FFXIVVenues.Veni.AI.Luis;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.AI.Clu
{
    internal class CluClient: ICluClient
    {

        private readonly CluConfiguration _config;
        private readonly ConversationAnalysisClient _client;

        public CluClient(CluConfiguration config)
        {
            this._config = config;
            var endpoint = new Uri(config.PredictionEndpoint);
            var credential = new AzureKeyCredential(config.RuntimeKey);

            this._client = new ConversationAnalysisClient(endpoint, credential);
        }

        public async Task<CluPrediction> AnalyzeAsync(string query, ulong userId)
        {
            if (query.Length > 60)
                return new CluPrediction { TopIntent = IntentNames.None };
            var data = new
            {
                analysisInput = new
                {
                    conversationItem = new
                    {
                        text = query,
                        language = "en",
                        participantId = userId.ToString(),
                        id = IdHelper.GenerateId(),
                    }
                },
                parameters = new
                {
                    projectName = this._config.Project,
                    deploymentName =  this._config.Deployment,
                    stringIndexType = "Utf16CodeUnit",
                    verbose = true,
                },
                kind = "Conversation",
            };
            var response = await this._client.AnalyzeConversationAsync(RequestContent.Create(data));
            var result = response.Content.ToObjectFromJson<CluResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result.Result.Prediction;
        }

    }
}