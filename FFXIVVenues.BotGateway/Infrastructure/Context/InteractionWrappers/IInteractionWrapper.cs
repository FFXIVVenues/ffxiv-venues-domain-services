using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace FFXIVVenues.Veni.Infrastructure.Context.InteractionWrappers;

public interface IInteractionWrapper
{

    SocketUser User { get; }
    ISocketMessageChannel Channel { get; }

    string Content { get; }
    IInteractionDataWrapper InteractionData { get; }
    bool IsDM { get; }

    string GetArgument(string name);

    Task RespondAsync(string message = null, MessageComponent component = null, Embed embed = null);


}