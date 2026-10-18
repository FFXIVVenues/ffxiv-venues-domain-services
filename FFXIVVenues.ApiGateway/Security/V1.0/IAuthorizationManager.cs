namespace FFXIVVenues.ApiGateway.Security.V1._0;

public interface IAuthorizationManager
{
    string GetKeyString();
    AuthorizationKey GetKey(string key = null);
    bool IsAuthenticated();
    IAuthorizationCheck Check(string key = null);
}