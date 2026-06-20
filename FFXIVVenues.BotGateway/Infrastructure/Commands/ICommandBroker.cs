using System.Threading.Tasks;
using FFXIVVenues.BotGateway.Infrastructure.Context;

namespace FFXIVVenues.BotGateway.Infrastructure.Commands
{
    public interface ICommandBroker
    {
        void Add<TFactory, THandler>(string key, bool isMasterGuildCommand)
            where TFactory : ICommandFactory
            where THandler : ICommandHandler;

        void AddFromAssembly();
        Task HandleAsync(SlashCommandVeniInteractionContext context);
        Task RegisterAllGlobalCommandsAsync();
        Task RegisterMasterGuildCommandsAsync();
    }

}
