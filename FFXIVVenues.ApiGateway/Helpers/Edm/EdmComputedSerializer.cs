using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace FFXIVVenues.ApiGateway.Helpers.Edm;

internal class EdmComputedSerializer(IODataSerializerProvider provider)
    : ODataResourceSerializer(provider)
{
    public override ODataProperty CreateStructuralProperty(IEdmStructuralProperty property, ResourceContext context)
    {
        var metaData = property.GetMetaData();
        if (metaData is not { ValueFactory: not null })
            return base.CreateStructuralProperty(property, context);

        return new ODataProperty
        {
            Name = property.Name,
            Value = metaData.ValueFactory(property, context)
        };
    }
}
