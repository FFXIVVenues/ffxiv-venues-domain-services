namespace FFXIVVenues.ApiGateway.Security.V1._0;

public interface ISecurityScoped
{
    bool Approved { get; }
    string ScopeKey { get; }
}