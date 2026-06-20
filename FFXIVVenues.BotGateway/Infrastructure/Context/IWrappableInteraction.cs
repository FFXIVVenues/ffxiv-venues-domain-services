namespace FFXIVVenues.BotGateway.Infrastructure.Context
{
    public interface IWrappableInteraction
    {

        VeniInteractionContext ToWrappedInteraction();

    }
}