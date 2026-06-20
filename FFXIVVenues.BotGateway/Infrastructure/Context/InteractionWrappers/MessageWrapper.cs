using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;


namespace FFXIVVenues.Veni.Infrastructure.Context.InteractionWrappers;

public class MessageWrapper : IInteractionWrapper
{

    public SocketUser User => _message?.Author;
    public ISocketMessageChannel Channel => _message?.Channel;
    public string Content => _message?.CleanContent;
    public IInteractionDataWrapper InteractionData => null;
    public bool IsDM => this._message.Channel is IDMChannel;

    private readonly SocketMessage _message;

    public MessageWrapper(SocketMessage message)
    {
        this._message = message;
    }

    public Task RespondAsync(string message = null, MessageComponent component = null, Embed embed = null)
    {
        return _message.Channel.SendMessageAsync(message, components: component, embed: embed);
    }

    public string GetArgument(string name) =>
        null;
}
