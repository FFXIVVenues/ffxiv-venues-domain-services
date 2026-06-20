using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.ScheduleEntry;

class BiweeklyCommencementEntryState : ISessionState
{

    private Venue _venue;
    private int? _nowSettingDay;
    private DateTimeOffset[] _nextFourPossibleCommencements;
    private Day _day;

    public Task Enter(VeniInteractionContext c)
    {
        this._venue = c.Session.GetVenue();
        var timezone = c.Session.GetItem<string>(SessionKeys.TIMEZONE_ID);

        if (this._nowSettingDay == null)
        {
            this._nowSettingDay = c.Session.GetItem<int?>(SessionKeys.NOW_SETTING_SLOT);
            c.Session.ClearItem(SessionKeys.NOW_SETTING_SLOT);
        }

        this._nowSettingDay ??= 0;

        this._day = _venue.Schedule[this._nowSettingDay.Value].Day;
        var message = string.Format(VenueControlStrings.AskForCommencementMessage,
            this._day);

        var select = new SelectMenuBuilder()
            .WithCustomId(c.Session.RegisterComponentHandler(Handle, ComponentPersistence.ClearRow));
        this._nextFourPossibleCommencements = DateHelper.GetNextNDatesForDay(4, this._day.ToDayOfWeek(), timezone);
        for (int i = 0; i < this._nextFourPossibleCommencements.Length; i++)
        {
            var possibleCommencement = this._nextFourPossibleCommencements[i];
            select.AddOption(possibleCommencement.ToString("dddd dd MMMM"), i.ToString());
        }
            
        return c.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {message}",
            new ComponentBuilder().WithSelectMenu(select).WithBackButton(c).Build());
    }

    private Task Handle(ComponentVeniInteractionContext c)
    {
        var selection = c.Interaction.Data.Values.Single();
        var selectedCommencement = this._nextFourPossibleCommencements[int.Parse(selection)];
        var schedule = this._venue.Schedule[this._nowSettingDay!.Value];
        schedule.Commencing = selectedCommencement;
        schedule.Interval = new ()
        {
            IntervalType = IntervalType.EveryXWeeks,
            IntervalArgument = 2
        };
        return NextState(c);
    }
    
    private Task NextState(ComponentVeniInteractionContext c)
    {
        var thisWasLastDay = this._nowSettingDay + 1 == _venue.Schedule.Count;
        if (!thisWasLastDay)
        {
            c.Session.SetItem(SessionKeys.NOW_SETTING_SLOT, this._nowSettingDay + 1);
            return c.Session.MoveStateAsync<BiweeklyCommencementEntryState>(c);
        }

        c.Session.ClearItem(SessionKeys.NOW_SETTING_SLOT);

        if (c.Session.InEditing())
            return c.Session.MoveStateAsync<ConfirmVenueSessionState>(c);
        return c.Session.MoveStateAsync<BannerEntrySessionState>(c);
    }
}