using FFXIVVenues.DomainSecurity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Math.EC.Rfc8032;
using ScottBrady.IdentityModel.Crypto;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.ApiGateway.Bootstrap;

internal static partial class Bootstrap
{
    public static void ConfigureAuthorization(this WebApplicationBuilder builder)
    {
        IdentityModelEventSource.ShowPII = true;
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer((options) =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Security:Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Security:Jwt:Audience"],
                    ValidateIssuerSigningKey = true,
                    ValidAlgorithms = new[] { ExtendedSecurityAlgorithms.EdDsa },
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = JwtRegisteredClaimNames.Sub
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        var hasAuthHeader = ctx.Request.Headers.TryGetValue("Authorization", out var authHeader);
                        if (hasAuthHeader)
                        {
                            var authHeaderStr = authHeader.ToString();
                            var isBearer = authHeaderStr.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);
                            if (isBearer)
                            {
                                var token = authHeaderStr.Substring("Bearer ".Length).Trim();
                                if (token.Contains('.'))
                                {
                                    ctx.Token = token;
                                    return Task.CompletedTask;
                                }
                            }
                        }

                        if (ctx.Request.Cookies.TryGetValue("token", out var cookieToken))
                        {
                            ctx.Token = cookieToken;
                            return Task.CompletedTask;
                        }

                        ctx.NoResult();
                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<SecurityKeyLoader>((options, keyLoader) =>
                options.TokenValidationParameters.IssuerSigningKey = keyLoader.LoadEdDsaKey())
;

        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });
    }
}
