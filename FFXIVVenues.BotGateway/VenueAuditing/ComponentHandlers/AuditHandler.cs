using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;

namespace FFXIVVenues.Veni.VenueAuditing.ComponentHandlers;

public class AuditHandler : IComponentHandler
{

    // Change this key and any existing buttons linked to this will die
    public static string Key => "CONTROL_AUDIT";

    private readonly IAuthorizer _authorizer;
    private readonly IApiService _apiService;
    private readonly IVenueAuditService _auditService;

    public AuditHandler(IAuthorizer authorizer, IApiService apiService, IVenueAuditService auditService)
    {
        this._authorizer = authorizer;
        this._apiService = apiService;
        this._auditService = auditService;
    }
    
    public async Task HandleAsync(ComponentVeniInteractionContext context, string[] args)
    {
        var user = context.Interaction.User.Id;
        var venueId = args[0];
        var force = args[1] == "true";
        var retry = !string.IsNullOrEmpty(args[2]);
        var retryId = args[2];
        var venue = await this._apiService.GetVenueAsync(venueId);
        
        if (!this._authorizer.Authorize(user, Permission.AuditVenue, venue).Authorized)
            return;
        
        _ = context.Interaction.ModifyOriginalResponseAsync(props =>
            props.Components = new ComponentBuilder().Build());

        VenueAudit audit;
        if (retry)
        {
            var auditRecord = await this._auditService.GetAudit(retryId);
            if (auditRecord == null)
            {
                await context.Interaction.Channel.SendMessageAsync($"Sorry, that audit no longer exists. 😢");
                return;
            }
            audit = this._auditService.CreateAuditFor(venue, auditRecord);
        }
        else
        {
            audit = this._auditService.CreateAuditFor(venue,
                roundId: null,
                context.Interaction.Channel.Id,
                context.Interaction.User.Id);
        }

        if (!force && !await audit.IsAuditRequired())
        {
            await context.Interaction.Channel.SendMessageAsync("This venue has been audited recently, should I audit it anyway? 🤔", 
                components: new ComponentBuilder()
                    .WithButton(new ButtonBuilder("Audit anyway").WithStaticHandler(AuditHandler.Key, venueId, "true", retryId).WithStyle(ButtonStyle.Primary))
                    .WithButton(new ButtonBuilder("Cancel").WithSessionHandler(context.Session, 
                        c => context.Interaction.Channel.SendMessageAsync($"Oki, we'll leave it. 😊"),
                        ComponentPersistence.ClearRow).WithStyle(ButtonStyle.Secondary))
                    .Build());
            return;
        }

        var result = await audit.AuditAsync(force);
        if (result == VenueAuditStatus.AwaitingResponse)
            await context.Interaction.Channel.SendMessageAsync("Okay, I've messaged the manager(s)! 🥰");
        else if (result == VenueAuditStatus.Failed)
            await context.Interaction.Channel.SendMessageAsync($"I couldn't message any of the managers. 😢");
    }
    
}