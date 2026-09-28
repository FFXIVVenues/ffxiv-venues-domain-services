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
using System.Security.Claims;

namespace FFXIVVenues.ApiGateway.Controllers;

[ApiController]
[Route("login")]
[ApiExplorerSettings(IgnoreApi = true)]
public class LoginController(Signer signer, IConfiguration config) : ControllerBase
{
    private static MemoryCache nonceCache = new(new MemoryCacheOptions());

    [HttpGet("whoami")]
    public ActionResult Index()
    {
        return Ok("Logged in as " + this.HttpContext.User.Identity?.Name ?? "nobody");
    }

    [HttpGet("sso")]
    [AllowAnonymous]
    public ActionResult Sso(
        [FromQuery] long userId,
        [FromQuery] string redirect,
        [FromQuery] string nonce,
        [FromQuery] long timestamp,
        [FromQuery] string signature)
    {
        var expectedSignature = signer.Sign(this.Request.Host + this.Request.Method + this.Request.Path + userId + redirect + nonce + timestamp);
        if (expectedSignature != signature)
            return BadRequest();

        var expiry = DateTimeOffset.FromUnixTimeSeconds(timestamp).AddMinutes(1);
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

        this.Login(userId);

        return Redirect(redirectUri.ToString());
    }

    private void Login(long userId)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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
            Expires = DateTimeOffset.UtcNow.AddHours(8),
            Path = "/"
        });
    }
}
