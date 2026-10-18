using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace FFXIVVenues.ApiGateway.Helpers.Edm;

public static class ServiceExtensions
{
    public static IServiceCollection AddVenuesSerializer(this IServiceCollection services) =>
        services.AddSingleton<ODataResourceSerializer, EdmComputedSerializer>()
            .AddSingleton<ODataResourceDeserializer, EdmComputedDeserializer>();
}