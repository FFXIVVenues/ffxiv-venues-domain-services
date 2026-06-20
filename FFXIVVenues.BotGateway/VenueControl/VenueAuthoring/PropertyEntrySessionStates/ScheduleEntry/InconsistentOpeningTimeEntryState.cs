using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.ScheduleEntry;

class InconsistentOpeningTimeEntrySessionState(IAuthorizer authorizer) : ISessionState
{

    private static Regex _regex = new Regex("(?<hour>[0-9]|(1[0-2]))(:?(?<minute>[0-5][0-9]))? ?(?<meridiem>am|pm)");

    private Venue _venue;
    private string _timeZoneId;
    private bool? _nowSettingClosing;
    private int? _nowSettingDay;
    private bool _sentUnstaffedWarning;

    public Task Enter(VeniInteractionContext c)
    {
        c.Session.RegisterMessageHandler(this.OnMessageReceived);

        this._venue = c.Session.GetVenue();
        this._timeZoneId = c.Session.GetItem<string>(SessionKeys.TIMEZONE_ID);

        if (this._nowSettingClosing == null)
        {
            this._nowSettingClosing = c.Session.GetItem<bool?>(SessionKeys.NOW_SETTING_CLOSING);
            c.Session.ClearItem(SessionKeys.NOW_SETTING_CLOSING);
        }

        this._nowSettingClosing ??= false;
        if (this._nowSettingDay == null)
        {
            this._nowSettingDay = c.Session.GetItem<int?>(SessionKeys.NOW_SETTING_SLOT);
            c.Session.ClearItem(SessionKeys.NOW_SETTING_SLOT);
        }

        this._nowSettingDay ??= 0;
        var message = !this._nowSettingClosing.Value ? VenueControlStrings.AskForOpenTimeOnDayMessage : VenueControlStrings.AskForCloseTimeOnDayMessage;
        var isDm = c.Interaction.Channel is IDMChannel;
        var openingForDayMessage = string.Format(message, _venue.Schedule[this._nowSettingDay.Value].Day);
            
        return c.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {openingForDayMessage}",
            new ComponentBuilder().WithBackButton(c).Build(),
            isDm ? null : new EmbedBuilder().WithDescription("**@ Veni Ki** with your time").WithColor(Color.Blue).Build());
    }

    public Task OnMessageReceived(MessageVeniInteractionContext c)
    {
        var message = c.Interaction.Content.StripMentions().ToLower();
        var match = _regex.Match(message);
        if (!match.Success)
            return c.Interaction.Channel.SendMessageAsync($"Sorry, I didn't understand that, could you write in 12-hour format? Like 12am, or 7:30pm?");

        var hour = ushort.Parse(match.Groups["hour"].Value);
        var minute = match.Groups["minute"].Success ? ushort.Parse(match.Groups["minute"].Value) : (ushort)0;
        var meridiem = match.Groups["meridiem"].Value;

        if (meridiem == "am" && hour == 12)
            hour = 0;
        else if (meridiem == "pm" && hour != 12)
            hour += 12;

        var opening = _venue.Schedule[this._nowSettingDay!.Value];
        if (!this._nowSettingClosing!.Value)
        {
            opening.Start = new Time { Hour = hour, Minute = minute, NextDay = false, TimeZone = _timeZoneId };
            c.Session.SetItem(SessionKeys.NOW_SETTING_SLOT, this._nowSettingDay);
            c.Session.SetItem(SessionKeys.NOW_SETTING_CLOSING, true);
            return c.Session.MoveStateAsync<InconsistentOpeningTimeEntrySessionState>(c);
        }

        if ( ! authorizer.Authorize(c.Interaction.Author.Id, Permission.SetLongSchedule, this._venue).Authorized)
        {
            var start = new TimeOnly(opening.Start.Hour, opening.Start.Minute);
            var end = new TimeOnly(hour, minute);
            var diff = end - start;
            var embed = this._sentUnstaffedWarning
                ? null
                : new EmbedBuilder()
                    .WithDescription(VenueControlStrings.UnstaffedOpeningsNotSupported)
                    .Build();
            this._sentUnstaffedWarning = true;
            if (diff > TimeSpan.FromHours(7)) 
                return c.Interaction.Channel.SendMessageAsync(VenueControlStrings.OpeningTooLong, embed: embed);
        }

        // setting closing time per day
        opening.End = new Time { Hour = hour, Minute = minute, NextDay = hour < opening.Start.Hour, TimeZone = _timeZoneId };

        var thisWasLastDay = this._nowSettingDay + 1 == _venue.Schedule.Count;
        if (!thisWasLastDay)
        {
            c.Session.SetItem(SessionKeys.NOW_SETTING_SLOT, this._nowSettingDay + 1);
            c.Session.SetItem(SessionKeys.NOW_SETTING_CLOSING, false);
            return c.Session.MoveStateAsync<InconsistentOpeningTimeEntrySessionState>(c);
        }

        c.Session.ClearItem(SessionKeys.NOW_SETTING_SLOT);
        c.Session.ClearItem(SessionKeys.NOW_SETTING_CLOSING);

        if (c.Session.IsScheduleBiweekly())
            return c.Session.MoveStateAsync<BiweeklyCommencementEntryState>(c);
            
        if (c.Session.IsScheduleMonthly())
            return c.Session.MoveStateAsync<MonthlyCommencementEntryState>(c);
                
        if (c.Session.InEditing())
            return c.Session.MoveStateAsync<ConfirmVenueSessionState>(c);
        return c.Session.MoveStateAsync<BannerEntrySessionState>(c);
    }
}