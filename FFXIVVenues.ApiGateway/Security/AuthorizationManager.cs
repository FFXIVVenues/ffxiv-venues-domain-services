using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace FFXIVVenues.ApiGateway.Security;

public class AuthorizationManager(
    IEnumerable<AuthorizationKey> authorizationkeys,
    IHttpContextAccessor httpContextAccessor)
    : IAuthorizationManager
{
    public bool IsAuthenticated() => this.GetKeyString() != null;
        
    public string GetKeyString()
    {
        var authorizationHeader = httpContextAccessor.HttpContext?.Request.Headers["Authorization"] ?? string.Empty;
        if (!authorizationHeader.Any())
            return null;

            
        var parsed = AuthenticationHeaderValue.TryParse(authorizationHeader.First(), out var authenticationHeader);

        if (parsed && authenticationHeader != null)
            return authenticationHeader.Parameter;

        return null;
    }

    public AuthorizationKey GetKey(string key = null)
    {
        if (key == null)
            key = this.GetKeyString();
        return authorizationkeys.FirstOrDefault(k => k.Key == key);
    }

    public IAuthorizationCheck Check(string key = null)
    {
        var authKey = this.GetKey(key);
        if (authKey == null)
            return new NonAuthorizationCheck();
        return new AuthorizationCheck(authKey);
    }

}