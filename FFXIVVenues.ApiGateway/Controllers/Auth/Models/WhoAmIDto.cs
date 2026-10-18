#pragma warning disable CS1591

using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using FFXIVVenues.ApiGateway.Security;

namespace FFXIVVenues.ApiGateway.Controllers.Auth.Models;

public record WhoAmIDto(
    [property: JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    ulong UserId,
    string Username,
    string Nickname,
    string AvatarUrl)
{
    public static WhoAmIDto FromCurrentUser(ICurrentUser user) =>
        new (user.Id, user.Username, user.Nickname, user.Claim(JwtRegisteredClaimNames.Picture));
}