using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.Extensions.Configuration;
using Microsoft.OData.Edm;

namespace FFXIVVenues.ApiGateway.Security;

public interface IAuthorizationProvider
{
    Authorization GetAuthorizationFor(ulong userId);
    Expression<Func<T, bool>> QueryToExpression<T>(string odataQuery);
    bool Matches<T>(T target, string odataQuery);
}

public class AuthorizationProvider(IEdmModel edmModel, IConfiguration config) : IAuthorizationProvider
{
    private readonly List<Authorization> _authorizations = 
        config.GetSection("Security:Authorizations").Get<List<Authorization>>() ?? [];

    public Authorization GetAuthorizationFor(ulong userId) =>
        _authorizations.FirstOrDefault(a => a.UserId == userId, 
            defaultValue: new Authorization(userId));

    public Expression<Func<T, bool>> QueryToExpression<T>(string odataQuery)
    {
        if (string.IsNullOrWhiteSpace(odataQuery) || odataQuery == Authorization.ODATA_FALSE)
            return (T v) => false;

        odataQuery = UrlEncoder.Default.Encode(odataQuery);
        var http = new DefaultHttpContext
        {
            Request =
            {
                QueryString = new QueryString("?$filter=" + odataQuery)
            }
        };
        var ctx = new ODataQueryContext(edmModel, typeof(T), path: null);
        var options = new ODataQueryOptions<T>(ctx, http.Request);
        var odataQuerySettings = new ODataQuerySettings
            { HandleNullPropagation = HandleNullPropagationOption.True, EnableCorrelatedSubqueryBuffering = true };
        var queryBinderContext = new QueryBinderContext(edmModel, odataQuerySettings, typeof(T));
        return new FilterBinder().BindFilter(options.Filter.FilterClause, queryBinderContext)
             as Expression<Func<T, bool>>;
    }

    public bool Matches<T>(T target, string odataQuery) =>
        this.QueryToExpression<T>(odataQuery).Compile().Invoke(target);

}

public record Authorization(ulong UserId)
{
    public const string ODATA_FALSE = "false";
    public string Edit { get; init; } = ODATA_FALSE;
    public string EditApproval { get; init; } = ODATA_FALSE;
    public string EditManagers { get; init; } = ODATA_FALSE;
    public string Delete { get; init; } = ODATA_FALSE;
    public bool UnlimitedSchedules { get; init; } = false;
    public bool UnlimitedVenues { get; init; } = false;
}
