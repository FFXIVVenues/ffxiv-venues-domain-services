using System.Threading.Tasks;
using Discord.WebSocket;
using FFXIVVenues.BotGateway.Utils.Broadcasting;
using FFXIVVenues.BotGateway.VenueRendering;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.BotGateway.VenueControl.VenueAuthoring.VenueApproval
{
    public interface IVenueApprovalService
    {

        Task<BroadcastReceipt> SendForApproval(Venue venue, string bannerUrl);

        Task<bool> ApproveVenueAsync(Venue venue, ulong approver);

        Task<bool> HandleComponentInteractionAsync(SocketMessageComponent context);

    }
}