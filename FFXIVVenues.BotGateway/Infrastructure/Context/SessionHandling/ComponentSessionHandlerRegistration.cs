using System;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Infrastructure.Context.SessionHandling
{
    internal class ComponentSessionHandlerRegistration
    {

        public Func<ComponentVeniInteractionContext, Task> Delegate { get; set; }
        public ComponentPersistence Persistence { get; set; }

        public ComponentSessionHandlerRegistration(Func<ComponentVeniInteractionContext, Task> @delegate, ComponentPersistence persistence)
        {
            Delegate = @delegate;
            Persistence = persistence;
        }

    }
}
