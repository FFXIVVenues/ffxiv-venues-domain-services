using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using FFXIVVenues.BotGateway.Infrastructure.Components;
using FFXIVVenues.BotGateway.Infrastructure.Context;
using FFXIVVenues.BotGateway.Utils.Broadcasting;
using FFXIVVenues.BotGateway.VenueRendering;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.BotGateway.VenueAuditing.ComponentHandlers.AuditResponse;

public abstract class BaseAuditHandler : IComponentHandler
{

    public abstract Task HandleAsync(ComponentVeniInteractionContext context, string[] args);

}