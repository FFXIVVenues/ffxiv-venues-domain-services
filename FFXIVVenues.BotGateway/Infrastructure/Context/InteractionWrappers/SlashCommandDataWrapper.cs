using System.Linq;
using Discord.WebSocket;

namespace FFXIVVenues.Veni.Infrastructure.Context.InteractionWrappers;

public class SlashCommandDataWrapper : IInteractionDataWrapper
{

    public string Name => _data.Name;

    private readonly SocketSlashCommandData _data;

    public SlashCommandDataWrapper(SocketSlashCommandData data)
    {
        _data = data;
    }

    public string GetArgument(string name) =>
        this._data.Options?.FirstOrDefault(o => o.Name == name)?.Value as string;
}
