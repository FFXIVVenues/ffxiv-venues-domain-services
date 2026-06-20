using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Color = Discord.Color;
using Image = SixLabors.ImageSharp.Image;

// ReSharper disable AccessToDisposedClosure

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates;

class BannerEntrySessionState(IAuthorizer authorizer, HttpClient httpClient) : ISessionState
{
    private const int BANNER_HEIGHT = 300;
    private const int BANNER_WIDTH = 600;

    public Task Enter(VeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();

        if (!authorizer.Authorize(c.Interaction.User.Id, Permission.EditPhotography, venue).Authorized)
            return c.Session.MoveStateAsync<ManagerEntrySessionState>(c);

        c.Session.RegisterMessageHandler(this.OnMessageReceived);
        c.Session.SetBackClearanceAmount(3);
        var isDm = c.Interaction.Channel is IDMChannel;
        return c.Interaction.RespondAsync("What **banner image** would you like to use?\nBanners are 600x300; I'll do the scaling/cropping for you :heart:.",
            new ComponentBuilder()
                .WithBackButton(c)
                .WithSkipButton<ManagerEntrySessionState, ConfirmVenueSessionState>(c)
                .Build(),
            isDm ? null : new EmbedBuilder()
                .WithDescription("**@ Veni Ki** with your image")
                .WithColor(Color.Blue)
                .Build()
            );
    }

    public async Task OnMessageReceived(MessageVeniInteractionContext c)
    {
        if (!c.Interaction.Attachments.Any())
        {
            await c.Interaction.Channel.SendMessageAsync(MessageRepository.DontUnderstandResponses.PickRandom() + " Could you send me an image you'd like to use for your banner?");
            return;
        }

        var attachment = c.Interaction.Attachments.First();
        if (!attachment.ContentType.StartsWith("image"))
        {
            await c.Interaction.Channel.SendMessageAsync("Sorry, could you send me an image file like a jpeg or png? :relaxed: ");
            return;
        }

        var stream = await httpClient.GetStreamAsync(attachment.ProxyUrl);
        var outStream = new MemoryStream();
        using (var image = await Image.LoadAsync(stream))
        {
            if (image.Width * image.Height > 15_728_640)
            {
                await c.Interaction.Channel.SendMessageAsync("Aaaah, my desk isn't big enough for this! 😓\n Can you send me that a _little_ smaller?");
                return;
            }

            image.Mutate(context =>
            {
                var scale = (float) BANNER_WIDTH / image.Width;
                if (image.Height * scale < BANNER_HEIGHT)
                {
                    scale = (float) BANNER_HEIGHT / image.Height;
                    context.Resize((int)(image.Width * scale), BANNER_HEIGHT);
                }
                else
                {
                    context.Resize(BANNER_WIDTH, (int)(image.Height * scale));
                }
            });

            image.Mutate(context =>
            {
                var xPoint = (image.Width - BANNER_WIDTH) / 2;
                var yPoint = (image.Height - BANNER_HEIGHT) / 2;
                context.Crop(new Rectangle(xPoint, yPoint, BANNER_WIDTH, BANNER_HEIGHT));
            });

            await image.SaveAsWebpAsync(outStream);
        }

        var component = new ComponentBuilder();
        component.WithButton("Looks good!", c.Session.RegisterComponentHandler(async cm =>
        {
            cm.Session.SetVenueBanner(cm.Interaction.Message.Attachments.First().ProxyUrl);
            if (cm.Session.InEditing())
                await cm.Session.MoveStateAsync<ConfirmVenueSessionState>(c);
            else
                await cm.Session.MoveStateAsync<ManagerEntrySessionState>(c);
        }, ComponentPersistence.ClearRow));
        component.WithButton("Let's try another!", c.Session.RegisterComponentHandler(cm => cm.Interaction.FollowupAsync("Alrighty, send over another image! :heart:"), ComponentPersistence.ClearRow), ButtonStyle.Secondary);
        var response = await c.Interaction.Channel.SendFileAsync(outStream, "banner.jpg", "How does this look? :heart:", components: component.Build());
    }

}