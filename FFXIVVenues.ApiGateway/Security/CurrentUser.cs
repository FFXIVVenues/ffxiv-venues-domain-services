#pragma warning disable CS1591

using Amazon.Auth.AccessControlPolicy;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FFXIVVenues.ApiGateway.Security;

public interface ICurrentUser
{

    ClaimsPrincipal Principal { get; }
    bool IsAuthenticated { get; }
    string Claim(string type);

    ulong Id { get; }
    string Nickname { get; }
    string Username { get; }
}

public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public ClaimsPrincipal Principal => accessor.HttpContext?.User;
    public string Claim(string type) => Principal?.FindFirst(type)?.Value;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
    public ulong Id => ulong.Parse(Claim(JwtRegisteredClaimNames.Sub));
    public string Username => Claim(JwtRegisteredClaimNames.PreferredUsername);
    public string Nickname => Claim(JwtRegisteredClaimNames.Nickname);
    public string AvatarUrl => Claim(JwtRegisteredClaimNames.Picture);
}
