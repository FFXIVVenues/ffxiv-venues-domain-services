using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl;

// Create a more type safe Session store, maybe Generics
public static class SessionExtensions
{
    public static Venue GetVenue(this Session session) =>
        session.GetItem<Venue>(SessionKeys.VENUE);

    public static void SetVenue(this Session session, Venue venue) =>
        session.SetItem(SessionKeys.VENUE, venue);
    
    public static string GetVenueBanner(this Session session) =>
        session.GetItem<string>(SessionKeys.BANNER_URL);
    
    public static void SetVenueBanner(this Session session, string bannerUri) =>
        session.SetItem(SessionKeys.BANNER_URL, bannerUri);

    public static bool IsNewVenue(this Session session) =>
        session.GetItem<bool>(SessionKeys.IS_NEW_VENUE);

    public static void SetIsNewVenue(this Session session, bool isNew = true) =>
        session.SetItem(SessionKeys.IS_NEW_VENUE, isNew);
    
    public static bool InEditing(this Session session) =>
        session.GetItem<bool>(SessionKeys.MODIFYING);

    public static void SetEditing(this Session session, bool editing = true) =>
        session.SetItem(SessionKeys.MODIFYING, editing);

    public static void SetScheduleAsBiweekly(this Session session) =>
        session.SetItem(SessionKeys.IS_BIWEEKLY_SCHEDULE, true);
    
    public static bool IsScheduleBiweekly(this Session session) =>
        session.GetItem<bool>(SessionKeys.IS_BIWEEKLY_SCHEDULE);
    
    public static void SetScheduleAsMonthly(this Session session) =>
        session.SetItem(SessionKeys.IS_MONTHLY_SCHEDULE, true);
    
    public static bool IsScheduleMonthly(this Session session) =>
        session.GetItem<bool>(SessionKeys.IS_MONTHLY_SCHEDULE);
    
}