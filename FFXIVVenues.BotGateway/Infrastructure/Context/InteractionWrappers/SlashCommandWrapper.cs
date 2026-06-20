using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;


namespace FFXIVVenues.Veni.Infrastructure.Context.InteractionWrappers;
    
public class SlashCommandWrapper : IInteractionWrapper
{
    public SocketUser User => _slashCommand?.User;
    public ISocketMessageChannel Channel => _slashCommand?.Channel;
    public string Content => null;
    public IInteractionDataWrapper InteractionData { get; set; }
    public bool IsDM => this._slashCommand.IsDMInteraction;

    private readonly SocketSlashCommand _slashCommand;

    public SlashCommandWrapper(SocketSlashCommand slashCommand)
    {
        this._slashCommand = slashCommand;
        this.InteractionData = new SlashCommandDataWrapper(slashCommand.Data);
    }

    public Task RespondAsync(string message = null, MessageComponent component = null, Embed embed = null)
    {
        return _slashCommand.HasResponded ?
               _slashCommand.Channel.SendMessageAsync(message, components: component, embed: embed) :
               _slashCommand.RespondAsync(message, components: component, embed: embed);
    }

    public string GetArgument(string name) =>
        this.InteractionData.GetArgument(name);
}
