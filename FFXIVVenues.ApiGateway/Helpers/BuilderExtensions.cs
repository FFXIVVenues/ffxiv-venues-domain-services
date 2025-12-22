using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace FFXIVVenues.ApiGateway.Helpers;

public static class BuilderExtensions
{
    extension(IServiceCollection serviceCollection) {
        public void AddVersionedOpenApi(ApiVersion apiVersion)
        {
            var versionAsString = $"v{apiVersion:VV}";
            serviceCollection.AddOpenApi( versionAsString, options =>
            {
                options.AddDocumentTransformer((document, context, _) =>
                {
                    document.Info.Title = "FFXIV Venues API Gateway";
                    document.Info.Version = versionAsString;
                    return Task.CompletedTask;
                });
            });
        }
    }
}