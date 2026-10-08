#pragma warning disable CS1591

using FFXIVVenues.ApiGateway.Security;
using FFXIVVenues.DomainSecurity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Converters;
using ScottBrady.IdentityModel.Crypto;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace FFXIVVenues.ApiGateway.Controllers;

[ApiController]
[Route("auth")]
[ApiExplorerSettings(IgnoreApi = true)]
public class AuthController(Signer signer, ICurrentUser user, IConfiguration config) : ControllerBase
{
    private static MemoryCache nonceCache = new(new MemoryCacheOptions());

    [HttpGet("whoami")]
    public LoggedInUser WhoAmI() =>
        new (user.Id, user.Username, user.Nickname, user.Claim(JwtRegisteredClaimNames.Picture));

    [HttpGet("sso")]
    [AllowAnonymous]
    public ActionResult Sso(
        [FromQuery(Name="user_id")] long userId,
        [FromQuery(Name = "user_username")] string username,
        [FromQuery(Name = "user_nickname")] string nickname,
        [FromQuery(Name = "user_picture")] string avatarUrl,
        [FromQuery] string redirect,
        [FromQuery] string nonce,
        [FromQuery] long timestamp,
        [FromQuery] string signature)
    {
        var expectedSignature = signer.Sign(this.Request.Host + this.Request.Method + this.Request.Path + userId + username + nickname + avatarUrl + redirect + nonce + timestamp);
        if (expectedSignature != signature)
            return BadRequest();

        var expiry = DateTimeOffset.FromUnixTimeSeconds(timestamp).AddMinutes(3);
        if (expiry < DateTimeOffset.UtcNow)
            return BadRequest();

        if (!nonceCache.TryGetValue(nonce, out _))
            nonceCache.Set(nonce, true, expiry);
        else
            return BadRequest();

        var allowedRedirectHosts = new List<string>();
        config.GetSection("Security:Sso:AllowedRedirectHosts").Bind(allowedRedirectHosts);
        var redirectUri = new Uri(redirect, UriKind.Absolute);
        if (!allowedRedirectHosts.Contains(redirectUri.Host))
            return BadRequest();

        if (userId == 0)
            return BadRequest();

        this.Login(userId, username, nickname, avatarUrl);

        return Redirect(redirectUri.ToString());
    }

    private void Login(long userId, string username, string nickname, string avatarUrl)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.PreferredUsername, username),
            new Claim(JwtRegisteredClaimNames.Nickname, nickname),
            new Claim(JwtRegisteredClaimNames.Picture, avatarUrl)
        };

        var token = new JwtSecurityToken(
            issuer: config["Security:Jwt:Issuer"],
            audience: config["Security:Jwt:Audience"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: signer.GetSigningCredentials());

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        Response.Cookies.Append("token", jwt, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Path = "/"
        });
    }
}

public record LoggedInUser(
    [property: JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)] ulong userId, 
    string username, string nickname, string avatarUrl);
