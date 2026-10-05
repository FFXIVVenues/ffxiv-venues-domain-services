using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.Edm.Vocabularies.V1;
using System.Linq;

namespace FFXIVVenues.ApiGateway.Helpers.Edm;

internal class EdmComputedDeserializer(IODataDeserializerProvider provider)
    : ODataResourceDeserializer(provider)
{
    public override void ApplyStructuralProperty(object resource, ODataProperty structuralProperty,
        IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
    {
        var edmProperty = structuredType.StructuredDefinition().FindProperty(structuralProperty.Name);

        // If property is marked as computed, don't apply the provided value
        if (edmProperty is not null && readContext.Model
            .FindVocabularyAnnotations<IEdmVocabularyAnnotation>(edmProperty, CoreVocabularyModel.ComputedTerm)
            .Any(a => a.Value is IEdmBooleanConstantExpression { Value: true }))
            return;

        base.ApplyStructuralProperty(resource, structuralProperty, structuredType, readContext);
    }
}
