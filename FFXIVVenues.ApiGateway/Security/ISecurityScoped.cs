namespace FFXIVVenues.ApiGateway.Security;

public interface ISecurityScoped
{
    bool Approved { get; }
    string ScopeKey { get; }
}