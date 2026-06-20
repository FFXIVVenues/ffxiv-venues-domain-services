using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.RabbitMQ;

namespace FFXIVVenues.Veni;

internal static partial class Bootstrap
{
    internal static void ConfigureRabbit(HostApplicationBuilder hostBuilder, Configurations config)
    {
        var rabbitServiceUrl = config.RabbitConfig.ServiceUrl;
        hostBuilder.UseWolverine(opts =>
        {
            opts.UseRabbitMq(rabbitServiceUrl)
                .DeclareExchange("FFXIVVenues.Flagging.Events", e => 
                    e.BindQueue("FFXIVVenues.Veni.EventsInbox.Flagging"))
                .AutoProvision();
            opts.ListenToRabbitQueue("FFXIVVenues.Veni.EventsInbox.Flagging");
        });
    }
}