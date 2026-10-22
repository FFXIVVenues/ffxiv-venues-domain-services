#pragma warning disable CS1591

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FFXIVVenues.ApiGateway.Controllers.Auth.Models;
using FFXIVVenues.ApiGateway.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Authorization = FFXIVVenues.ApiGateway.Security.Authorization;

namespace FFXIVVenues.ApiGateway.Controllers.Discord;

[ApiController]
[Route("discord")]
[ApiExplorerSettings(IgnoreApi = true)]
public class DiscordController(ICurrentUser user, IHttpClientFactory http) : ControllerBase
{
    private static MemoryCache userCache = new(new MemoryCacheOptions());
    private static DateTimeOffset unavailableUntil;

    [HttpGet("users/{id}")]
    public async Task<ActionResult<WhoAmIDto>> GetUser(ulong id)
    {
        if (user.Authorization is not { EditManagers: not null and not Authorization.ODATA_FALSE })
            return Forbid();

        if (userCache.TryGetValue(id, out WhoAmIDto known))
            return known;

        if (DateTimeOffset.UtcNow < unavailableUntil)
            return StatusCode(StatusCodes.Status503ServiceUnavailable);

        using var response = await http.CreateClient("Discord").GetAsync($"users/{id}", HttpContext.RequestAborted);
        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                var discordUser = await response.Content.ReadFromJsonAsync<DiscordUser>(HttpContext.RequestAborted);
                var found = new WhoAmIDto(id, discordUser.Username, discordUser.GlobalName ?? discordUser.Username, discordUser.Avatar is null ? null : $"https://cdn.discordapp.com/avatars/{id}/{discordUser.Avatar}.png?size=64");
                userCache.Set(id, found, TimeSpan.FromHours(1));
                return found;
            case HttpStatusCode.NotFound or HttpStatusCode.BadRequest:
                return NotFound();
            case HttpStatusCode.TooManyRequests:
                unavailableUntil = DateTimeOffset.UtcNow + (response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(5));
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            default:
                return StatusCode(StatusCodes.Status502BadGateway);
        }
    }

    private class DiscordUser
    {
        public string Username { get; set; }
        [JsonPropertyName("global_name")]
        public string GlobalName { get; set; }
        public string Avatar { get; set; }
    }
}