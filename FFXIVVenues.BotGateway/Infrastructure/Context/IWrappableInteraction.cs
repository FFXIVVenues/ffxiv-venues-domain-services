namespace FFXIVVenues.Veni.Infrastructure.Context
{
    public interface IWrappableInteraction
    {

        VeniInteractionContext ToWrappedInteraction();

    }
}