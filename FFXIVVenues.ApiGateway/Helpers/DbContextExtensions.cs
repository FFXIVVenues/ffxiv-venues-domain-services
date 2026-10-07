using System;
using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.DomainData.Context;
using Microsoft.AspNetCore.OData.Deltas;

namespace FFXIVVenues.ApiGateway.Helpers; 

internal static class DbContextExtensions 
{

    public static async Task LoadChangedNavigationsAsync(this DomainDataContext db, object entity, Delta delta)
    {
        foreach (var name in delta.GetChangedPropertyNames())
        {
            var nav = db.Entry(entity).Navigations
                .FirstOrDefault(n => string.Equals(n.Metadata.Name, name, StringComparison.OrdinalIgnoreCase));
            if (nav is { IsLoaded: false })
                await nav.LoadAsync();
        }
    }

}


