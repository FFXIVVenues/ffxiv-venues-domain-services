using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;


namespace FFXIVVenues.Veni.Infrastructure.Context.InteractionWrappers;

public class MessageComponentWrapper : IInteractionWrapper
{

    public SocketUser User => _messageComponent?.User;
    public ISocketMessageChannel Channel => _messageComponent?.Channel;
    public string Content => null;
    public IInteractionDataWrapper InteractionData { get; set; }
    public bool IsDM => this._messageComponent.IsDMInteraction;

    private SocketMessageComponent _messageComponent { get; }


    public MessageComponentWrapper(SocketMessageComponent messageComponent)
    {
        this._messageComponent = messageComponent;
        this.InteractionData = new ComponentDataWrapper(messageComponent.Data);
    }

    public Task RespondAsync(string message = null, MessageComponent component = null, Embed embed = null)
    {
        return _messageComponent.HasResponded ? 
            _messageComponent.Channel.SendMessageAsync(message, components: component, embed: embed) : 
            _messageComponent.RespondAsync(message, components: component, embed: embed);
    }

    public string GetArgument(string name) =>
        this.InteractionData.GetArgument(name);
}
