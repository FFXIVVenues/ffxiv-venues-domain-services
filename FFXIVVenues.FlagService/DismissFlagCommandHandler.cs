using FFXIVVenues.DomainData.Context;
using FFXIVVenues.DomainData.Entities.Flags;
using FFXIVVenues.FlagService.Client.Commands;
using FFXIVVenues.FlagService.Client.Events;
using Wolverine;

namespace FFXIVVenues.FlagService;

public class DismissFlagCommandHandler(IMessageBus bus, DomainDataContext domainData, ILogger<DismissFlagCommandHandler> logger)
{
    public ValueTask Handle(DismissFlagCommand command)
    {
        var flag = domainData.Flags.FirstOrDefault(f => f.Id == command.FlagId);
        if (flag == null)
        {
            logger.LogWarning("Could not dismiss flag {FlagId}, could not find flag with that id", command.FlagId);
            return ValueTask.CompletedTask;
        }

        flag.Resolution = FlagResolution.Dismissed;
        flag.ResolutionDate = DateTimeOffset.UtcNow;
        flag.ResolvedBy = command.DismissedBy;
        
        domainData.SaveChanges();
        
        logger.LogInformation("Flag {FlagId} for venue {VenueId} dismissed", flag.Id, flag.VenueId);
        
        return bus.PublishAsync(new FlagDismissedEvent(
            command.FlagId,
            flag.VenueId,
            command.DismissedBy
        ));
    }
}
