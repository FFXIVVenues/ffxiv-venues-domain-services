using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation.Configuration;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.Utils.Broadcasting;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring;
using FFXIVVenues.Veni.VenueRendering;
using FFXIVVenues.VenueModels.Observability;

namespace FFXIVVenues.Veni.VenueObservations.CreatedWithoutSplash;

public class CreatedWithoutSplashObserver(
    IApiService apiService,
    IRepository repository,
    IVenueRenderer venueRenderer,
    IDiscordClient discordClient,
    NotificationsConfiguration config)
    : IApiObserver
{
    public async Task Handle(Observation observation)
    {
        var venue = await apiService.GetVenueAsync(observation.SubjectId);
        if (venue.BannerUri != null)
            return;

        var broadcastId = IdHelper.GenerateId(8);
        var broadcast =  new Broadcast(broadcastId, discordClient);
        broadcast.WithMessage($"Heyo photographers!\nI have a new venue in need of a splash banner! :heart:");
        broadcast.WithEmbed(venueRenderer.Render(venue));
        broadcast.WithComponent(bcc =>
        {
            var button = new ButtonBuilder();
            button.WithLabel("I'll take it!");
            button.WithStaticHandler(VolunteerComponentHandler.Key, broadcastId, venue.Id);
            button.WithStyle(ButtonStyle.Success);
            
            var builder = new ComponentBuilder();
            builder.WithButton(button);

            return builder;
        });
        var dc = FfxivWorlds.GetRegionForDataCenter(venue.Location?.DataCenter);
        var recipients = config.MissingSplash.ResolveFor(dc);
        var broadcastReceipt = await broadcast.SendToAsync(recipients);
        await repository.UpsertAsync(broadcastReceipt);
    }
}