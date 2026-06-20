namespace FFXIVVenues.Veni.Infrastructure.Context.InteractionWrappers;

public interface IInteractionDataWrapper
{
    string Name { get; }

    string GetArgument(string name);

}
