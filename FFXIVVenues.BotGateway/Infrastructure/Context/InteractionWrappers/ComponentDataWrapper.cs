using Discord.WebSocket;

namespace FFXIVVenues.Veni.Infrastructure.Context.InteractionWrappers;

public class ComponentDataWrapper : IInteractionDataWrapper
{

    private readonly SocketMessageComponentData _data;

    public string Name => _data.CustomId;
    
    public ComponentDataWrapper(SocketMessageComponentData data)
    {
        _data = data;
    }

    public string GetArgument(string name) =>
        this._data.Value;
    
}
