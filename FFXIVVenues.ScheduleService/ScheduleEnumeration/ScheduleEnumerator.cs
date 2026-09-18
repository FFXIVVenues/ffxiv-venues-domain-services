using FFXIVVenues.DomainData.Entities.Venues;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TimeZoneConverter;

namespace FFXIVVenues.ScheduleService.ScheduleEnumeration;

public class ScheduleEnumerator : IEnumerator<FromTo>
{

    private readonly Schedule _schedule;
    private readonly DateTimeOffset _from;
    private readonly int _interval;
    private readonly TimeZoneInfo _timeZone;

    public ScheduleEnumerator(Schedule schedule, DateTimeOffset? fromFallback = null)
    {
        this._schedule = schedule;
        this._from = this._schedule.Commencing ?? fromFallback
                   ?? throw new ArgumentNullException(nameof(fromFallback), "No start point for enumeration given; provide either a fallback argument or a schedule with a IntervalFrom.");
        this._interval = this._schedule.IntervalArgument;
        this._timeZone = TZConvert.GetTimeZoneInfo(this._schedule.TimeZone);
    }

    public bool MoveNext()
    {
        if (this._schedule.IntervalType == FFXIVVenues.VenueModels.IntervalType.EveryXWeeks)
            return this.MoveNextByXWeeks();
        if (this._schedule.IntervalType == FFXIVVenues.VenueModels.IntervalType.EveryXthDayOfTheMonth
            && this._schedule.IntervalArgument > 0)
            return this.MoveNextByXthDayOfMonth();
        if (this._schedule.IntervalType == FFXIVVenues.VenueModels.IntervalType.EveryXthDayOfTheMonth
            && this._schedule.IntervalArgument < 0)
            return this.MoveNextByXthDayOfMonthFromEnd();
        return false;
    }

    private bool MoveNextByXWeeks()
    {
        if (this.Current != null)
        {
            this.Current = this.Current
                .AddDays(7 * this._interval)
                .SetOffset(_timeZone);
            return true;
        }

        var end = new Time(
            Hour: this._schedule.EndHour ?? (ushort)((this._schedule.StartHour + 3) % 24),
            Minute: this._schedule.EndMinute ?? this._schedule.StartMinute);

        var basis = this._from;
        var openingOffset = this._timeZone.GetUtcOffset(basis);
        if (basis.Offset != openingOffset)
            basis = basis.ToOffset(openingOffset);

        var dayOfWeek = this._schedule.Day;
        if (!end.IsLaterThan(this._schedule.StartHour, this._schedule.StartMinute))
            dayOfWeek = dayOfWeek.Next();
        var daysUntilTarget = (dayOfWeek.ToDayOfWeek() - basis.DayOfWeek + 7) % 7;
        var firstClosing = new DateTimeOffset(basis.Year, basis.Month, basis.Day, end.Hour, end.Minute, 0, basis.Offset).AddDays(daysUntilTarget);
        if (firstClosing <= basis)
            firstClosing = firstClosing.AddDays(7);

        var firstOpening = new DateTimeOffset(firstClosing.Year, firstClosing.Month, firstClosing.Day,
            this._schedule.StartHour, this._schedule.StartMinute, 0, firstClosing.Offset);
        if (firstOpening > firstClosing)
            firstOpening = firstOpening.AddDays(-1);

        this.Current = new FromTo(firstOpening, firstClosing).SetOffset(_timeZone);
        return true;
    }

    private bool MoveNextByXthDayOfMonth()
    {
        if (this.Current != null)
        {
            this.Current = this.Current
                .AddMonths(1)
                .ResetDay()
                .RollToDay(this._schedule.Day)
                .AddDays(7 * (this._schedule.IntervalArgument - 1))
                .SetOffset(_timeZone);
            return true;
        }

        var end = new Time(
            Hour: this._schedule.EndHour ?? (ushort)((this._schedule.StartHour + 3) % 24),
            Minute: this._schedule.EndMinute ?? this._schedule.StartMinute);

        var basis = this._from;
        var offset = this._timeZone.GetUtcOffset(basis);
        if (basis.Offset != offset)
            basis = basis.ToOffset(offset);

        var initialStart = new DateTimeOffset(basis.Year, basis.Month, 1,
            this._schedule.StartHour, this._schedule.StartMinute, 0, offset);
        var initialEnd = new DateTimeOffset(basis.Year, basis.Month, 1,
            end.Hour, end.Minute, 0, offset);
        if (initialStart > initialEnd)
            initialEnd = initialEnd.AddDays(1);

        this.Current = new FromTo(initialStart, initialEnd)
            .RollToDay(this._schedule.Day)
            .AddDays(7 * (this._schedule.IntervalArgument - 1))
            .SetOffset(_timeZone);

        if (this.Current!.End < basis)
            return this.MoveNextByXthDayOfMonth();

        return true;
    }

    private bool MoveNextByXthDayOfMonthFromEnd()
    {
        var intervalArgument = Math.Abs(this._schedule.IntervalArgument);
        if (this.Current != null)
        {
            // Logic to change current day might change depending on how you implement .AddDaysFromEnd
            this.Current = this.Current
                .AddMonths(1)
                .MaxDay()
                .RollBackToDay(this._schedule.Day)
                .RemoveDays(7 * (intervalArgument - 1))
                .SetOffset(_timeZone);
            return true;
        }

        var end = new Time(
            Hour: this._schedule.EndHour ?? (ushort)((this._schedule.StartHour + 3) % 24),
            Minute: this._schedule.EndMinute ?? this._schedule.StartMinute);

        var basis = this._from;
        var offset = this._timeZone.GetUtcOffset(basis);

        // Logic to find the initial start and end might change depending on 
        // how you implement .RollToDayFromEnd and .AddDaysFromEnd
        var initialStart = new DateTimeOffset(basis.Year, basis.Month,
            DateTime.DaysInMonth(basis.Year, basis.Month),
            this._schedule.StartHour, this._schedule.StartMinute, 0, offset);
        var initialEnd = new DateTimeOffset(basis.Year, basis.Month,
            DateTime.DaysInMonth(basis.Year, basis.Month),
            end.Hour, end.Minute, 0, offset);
        if (initialStart > initialEnd)
            initialEnd = initialEnd.AddDays(1);

        this.Current = new FromTo(initialStart, initialEnd)
            .RollBackToDay(this._schedule.Day)
            .RemoveDays(7 * (intervalArgument - 1))
            .SetOffset(_timeZone);

        if (this.Current!.End < basis)
            return this.MoveNextByXthDayOfMonthFromEnd();

        return true;
    }

    public void Reset() =>
        this.Current = null;

    public FromTo? Current { get; private set; }

    object? IEnumerator.Current => Current;

    public void Dispose() { }

    record Time(ushort Hour, ushort Minute)
    {
        public bool IsLaterThan(ushort hour, ushort minute) =>
            Hour > hour || (Hour == hour && Minute > minute);
    };
}

