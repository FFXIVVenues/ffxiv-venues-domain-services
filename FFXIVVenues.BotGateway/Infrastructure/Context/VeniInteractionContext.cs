using System;
using System.Linq;
using Discord.WebSocket;
using FFXIVVenues.Veni.AI.Clu.CluModels;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionWrappers;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;


namespace FFXIVVenues.Veni.Infrastructure.Context
{

    public abstract class VeniInteractionContext<T>(T message, DiscordSocketClient client, Session conversation)
        : IVeniInteractionContext
        where T : class
    {

        public T Interaction { get; } = message;
        public DiscordSocketClient Client { get; } = client;
        public Session Session { get; } = conversation;
        public IDisposable TypingHandle { get; set; }
    }

    public class VeniInteractionContext(IInteractionWrapper m,
        DiscordSocketClient dsc,
        Session sc,
        CluPrediction prediction = null) : VeniInteractionContext<IInteractionWrapper>(m, dsc, sc)
    {
        public CluPrediction Prediction { get; set; } = prediction;
    }

    public class MessageVeniInteractionContext(
        SocketMessage m,
        DiscordSocketClient dsc,
        Session sc)
        : VeniInteractionContext<SocketMessage>(m, dsc, sc), IWrappableInteraction
    {
    
        public CluPrediction Prediction { get; set; }
    
        public VeniInteractionContext ToWrappedInteraction() =>
            new (new MessageWrapper(this.Interaction),
                                   this.Client,
                                   this.Session,
                                   this.Prediction);
    
    }

    public class ComponentVeniInteractionContext(
        SocketMessageComponent m,
        DiscordSocketClient dsc,
        Session sc)
        : VeniInteractionContext<SocketMessageComponent>(m, dsc, sc), IWrappableInteraction
    {
        public VeniInteractionContext ToWrappedInteraction() =>
            new (new MessageComponentWrapper(this.Interaction),
                                   this.Client,
                                   this.Session);

    }

    public class SlashCommandVeniInteractionContext(
        SocketSlashCommand m,
        DiscordSocketClient dsc,
        Session sc)
        : VeniInteractionContext<SocketSlashCommand>(m, dsc, sc), IWrappableInteraction
    {
        public VeniInteractionContext ToWrappedInteraction() =>
            new (new SlashCommandWrapper(this.Interaction),
                                   this.Client,
                                   this.Session);

    }

}
