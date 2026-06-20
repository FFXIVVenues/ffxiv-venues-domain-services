using FFXIVVenues.DomainData.Context;
using FFXIVVenues.DomainData.Entities.Flags;
using FFXIVVenues.FlagService.Client.Commands;
using FFXIVVenues.FlagService.Client.Events;
using Wolverine;

namespace FFXIVVenues.FlagService;

public class ResolveFlagCommandHandler(IMessageBus bus, DomainDataContext domainData, ILogger<ResolveFlagCommandHandler> logger)
{
    public ValueTask Handle(ResolveFlagCommand command)
    {
        var flag = domainData.Flags.FirstOrDefault(f => f.Id == command.FlagId);
        if (flag == null)
        {
            logger.LogWarning("Could not resolve flag {FlagId}, could not find flag with that id", command.FlagId);
            return ValueTask.CompletedTask;
        }

        flag.Resolution = FlagResolution.Resolved;
        flag.ResolutionDate = DateTimeOffset.UtcNow;
        flag.ResolvedBy = command.ResolvedBy;
        
        domainData.SaveChanges();
        
        logger.LogInformation("Flag {FlagId} for venue {VenueId} resolved", flag.Id, flag.VenueId);
        
        return bus.PublishAsync(new FlagResolvedEvent(
            command.FlagId,
            flag.VenueId,
            command.ResolvedBy
        ));
    }
}
