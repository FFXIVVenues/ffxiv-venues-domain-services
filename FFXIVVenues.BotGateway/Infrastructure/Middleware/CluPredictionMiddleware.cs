using System;
using System.Threading.Tasks;
using FFXIVVenues.BotGateway.AI.Clu;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Infrastructure.Intent;
using FFXIVVenues.BotGateway.Utils;
using FFXIVVenues.BotGateway.AI.Clu.CluModels;
using Kana.Pipelines;

namespace FFXIVVenues.BotGateway.Infrastructure.Middleware;

internal class CluPredictionMiddleware(ICluClient cluClient) : IMiddleware<MessageVeniInteractionContext>
{
    public async Task ExecuteAsync(MessageVeniInteractionContext context, Func<Task> next)
    {
        var query = context.Interaction.Content.StripMentions(context.Client.CurrentUser.Id);
        if (string.IsNullOrWhiteSpace(query))
        {
            context.Prediction = new CluPrediction { TopIntent = IntentNames.None };
            await next();
            return;
        }

        context.Prediction = await cluClient.AnalyzeAsync(query, context.Interaction.Author.Id);
        await next();
    }
}