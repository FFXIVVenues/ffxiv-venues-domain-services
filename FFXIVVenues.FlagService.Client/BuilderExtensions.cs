using FFXIVVenues.FlagService.Client.Commands;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Wolverine.RabbitMQ;

namespace FFXIVVenues.FlagService.Client;

public static class BuilderExtensions
{
    public static IServiceCollection AddFlagService(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<FlagServiceClient>();
        return serviceCollection;
    }
    
    public static WolverineOptions AddFlagServiceMessages(this WolverineOptions wolverineOptions)
    {
        wolverineOptions
            .PublishMessage<FlagVenueCommand>().ToRabbitQueue("FFXIVVenues.Flagging.Commands");
        wolverineOptions.PublishMessage<ResolveFlagCommand>().ToRabbitQueue("FFXIVVenues.Flagging.Commands");
        wolverineOptions.PublishMessage<DismissFlagCommand>().ToRabbitQueue("FFXIVVenues.Flagging.Commands");
        return wolverineOptions;
    }
}