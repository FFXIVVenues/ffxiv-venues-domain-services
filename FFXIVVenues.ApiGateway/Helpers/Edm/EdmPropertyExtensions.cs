using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FFXIVVenues.ApiGateway.Helpers.Edm;

internal static class EdmPropertyExtensions
{

    static readonly ConcurrentDictionary<string, EdmPropertyMetaData> _propertyMeta = new();

    public static T Computed<T>(this T prop) where T : StructuralPropertyConfiguration
    {
        prop.HasComputed().IsComputed(true);
        return prop;
    }

    public static T Computed<T>(this T prop, Func<IEdmStructuralProperty, ResourceContext, object> valueFactory) where T: StructuralPropertyConfiguration
    {
        prop.HasComputed().IsComputed(true);
        var meta = _propertyMeta.GetOrAdd($"{prop.DeclaringType.FullName}.{prop.Name}", (_) => new());
        meta.ValueFactory = valueFactory;
        return prop;
    }

    public static EdmPropertyMetaData? GetMetaData(this IEdmStructuralProperty property) =>
        _propertyMeta.GetValueOrDefault($"{property.DeclaringType.FullTypeName()}.{property.Name}");

}

internal class EdmPropertyMetaData
{
    public Func<IEdmStructuralProperty, ResourceContext, object> ValueFactory { get; set; }
}