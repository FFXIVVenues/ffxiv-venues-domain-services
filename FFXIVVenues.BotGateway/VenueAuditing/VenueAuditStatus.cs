namespace FFXIVVenues.Veni.VenueAuditing;

public enum VenueAuditStatus
{
    Pending = 0,
    AwaitingResponse = 1,
    RespondedEdit = 2,
    RespondedConfirmed = 3,
    RespondedDelete = 4,
    RespondedClose = 5,
    EditedLater = 8,
    DeletedLater = 9,
    ClosedLater = 10,
    Skipped = 6,
    Failed = 7
}

public static class VenueAuditStatusExtensions
{
    public static bool IsResponded(this VenueAuditStatus status) =>
        status is VenueAuditStatus.RespondedClose 
               or VenueAuditStatus.RespondedConfirmed 
               or VenueAuditStatus.RespondedDelete 
               or VenueAuditStatus.RespondedEdit
               or VenueAuditStatus.EditedLater
               or VenueAuditStatus.DeletedLater
               or VenueAuditStatus.ClosedLater;
    
    public static bool IsRespondedByLateAction(this VenueAuditStatus status) =>
        status is VenueAuditStatus.EditedLater
               or VenueAuditStatus.DeletedLater
               or VenueAuditStatus.ClosedLater;
}