using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.AI.Clu.CluModels;
using FFXIVVenues.Veni.AI.Luis;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.Utils;
using Kana.Pipelines;

namespace FFXIVVenues.Veni.Infrastructure.Middleware;

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