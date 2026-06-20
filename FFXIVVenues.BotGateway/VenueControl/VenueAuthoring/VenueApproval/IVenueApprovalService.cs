using System.Threading.Tasks;
using Discord.WebSocket;
using FFXIVVenues.Veni.Utils.Broadcasting;
using FFXIVVenues.Veni.VenueRendering;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueApproval
{
    public interface IVenueApprovalService
    {

        Task<BroadcastReceipt> SendForApproval(Venue venue, string bannerUrl);

        Task<bool> ApproveVenueAsync(Venue venue, ulong approver);

        Task<bool> HandleComponentInteractionAsync(SocketMessageComponent context);

    }
}