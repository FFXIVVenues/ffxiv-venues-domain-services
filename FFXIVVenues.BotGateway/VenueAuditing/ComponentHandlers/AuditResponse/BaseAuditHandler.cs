using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Utils.Broadcasting;
using FFXIVVenues.Veni.VenueRendering;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueAuditing.ComponentHandlers.AuditResponse;

public abstract class BaseAuditHandler : IComponentHandler
{

    public abstract Task HandleAsync(ComponentVeniInteractionContext context, string[] args);

}