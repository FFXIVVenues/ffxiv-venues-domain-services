using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        var meta = _propertyMeta.GetOrAdd($"{prop.DeclaringType.FullName}.{prop.PropertyInfo.Name}", (_) => new());
        meta.ValueFactory = valueFactory;
        return prop;
    }

    public static EdmPropertyMetaData? GetEdmPropertyMetaData(this PropertyInfo property) =>
        _propertyMeta.GetValueOrDefault($"{property.DeclaringType.FullName}.{property.Name}");

}

internal class EdmPropertyMetaData
{
    public Func<IEdmStructuralProperty, ResourceContext, object> ValueFactory { get; set; }
}