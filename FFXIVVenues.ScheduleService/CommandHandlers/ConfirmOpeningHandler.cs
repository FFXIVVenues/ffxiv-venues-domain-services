using FFXIVVenues.DomainData.Context;
using FFXIVVenues.DomainData.Entities.Venues;
using FFXIVVenues.ScheduleService.Client.Commands;
using Serilog;

namespace FFXIVVenues.ScheduleService.CommandHandlers;

public class ConfirmOpeningHandler(DomainDataContext db)
{
    public Task Handle(ConfirmOpening command)
    {
        Log.Information("Received confirm opening command for opening {openingId}", command.OpeningId);

        var opening = db.Openings.Find(command.OpeningId);
        if (opening is null)
        {
            Log.Information("Did not update opening {openingId}; opening did not exist", command.OpeningId);
            return Task.CompletedTask;
        }
        opening.Status = OpeningStatus.OpeningConfirmed;
        return db.SaveChangesAsync();
    }
}
