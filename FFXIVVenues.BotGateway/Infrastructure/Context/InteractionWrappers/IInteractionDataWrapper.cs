namespace FFXIVVenues.BotGateway.Infrastructure.Context.InteractionWrappers;

public interface IInteractionDataWrapper
{
    string Name { get; }

    string GetArgument(string name);

}
