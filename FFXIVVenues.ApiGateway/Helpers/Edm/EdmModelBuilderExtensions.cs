using System.Text.Json;
using Microsoft.OData.ModelBuilder;

namespace FFXIVVenues.ApiGateway.Helpers.Edm;

public static class EdmModelBuilderExtensions
{
    public static ODataModelBuilder EnableLowerCamelCase(this ODataModelBuilder builder)
    {
        foreach (var type in builder.StructuralTypes)
        foreach (var prop in type.Properties)
            prop.Name = JsonNamingPolicy.CamelCase.ConvertName(prop.Name);
        return builder;
    }
}