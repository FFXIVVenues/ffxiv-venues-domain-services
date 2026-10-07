using FFXIVVenues.ApiGateway.Helpers.Edm;
using FFXIVVenues.DomainData.Entities.Venues;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.ModelBuilder;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace FFXIVVenues.ApiGateway.Controllers.OData;

internal static class VenuesEdm
{

    public static ODataModelBuilder AddVenuesEdm(this ODataModelBuilder builder, IOptionsMonitor<MediaConfiguration> mediaConfig)
    {
        var venueSet = builder.EntitySet<Venue>("Venues");

        var venue = venueSet.EntityType;
        venue.HasKey(v => v.Id);
        venue.Property(v => v.Id).Computed();
        venue.Property(v => v.Name);
        venue.Property(v => v.Banner).Computed((prop, c) =>
        {
            var bannerKey = c.GetPropertyValue("banner") as string;
            var venueId = c.GetPropertyValue("id") as string;
            return bannerKey is null ? null
                : mediaConfig.CurrentValue.MediaUriTemplate.Replace("{venueId}", venueId).Replace("{bannerKey}", bannerKey);
        });
        venue.Property(v => v.Added).Computed();
        venue.Property(v => v.LastModified).Computed();
        venue.CollectionProperty(v => v.Description);
        venue.ContainsOptional(v => v.Location).AutomaticallyExpand(true);
        venue.Property(v => v.Website);
        venue.Property(v => v.Discord);
        venue.Property(v => v.Sfw);
        venue.Property(v => v.Approved).Computed();
        venue.ContainsMany(v => v.Schedule);
        venue.ContainsMany(v => v.ScheduleOverrides);
        venue.ContainsMany(v => v.Notices);
        venue.CollectionProperty(v => v.Managers).Computed();
        venue.CollectionProperty(v => v.Tags);

        var location = builder.EntityType<Location>();
        location.HasKey(l => l.Id);
        location.Property(l => l.Id).Computed();
        location.Property(l => l.DataCenter);
        location.Property(l => l.World);
        location.Property(l => l.District);
        location.Property(l => l.Ward);
        location.Property(l => l.Plot);
        location.Property(l => l.Apartment);
        location.Property(l => l.Room);
        location.Property(l => l.Subdivision);
        location.Property(l => l.Override);

        var intervalType = builder.EnumType<VenueModels.IntervalType>();
        intervalType.Member(VenueModels.IntervalType.EveryXWeeks);
        intervalType.Member(VenueModels.IntervalType.EveryXthDayOfTheMonth);

        var day = builder.EnumType<Day>();
        day.Member(Day.Monday);
        day.Member(Day.Tuesday);
        day.Member(Day.Wednesday);
        day.Member(Day.Thursday);
        day.Member(Day.Friday);
        day.Member(Day.Saturday);
        day.Member(Day.Sunday);

        var schedule = builder.EntityType<Schedule>();
        schedule.HasKey(s => new { s.Day, s.StartHour, s.StartMinute, s.IntervalType, s.IntervalArgument });
        schedule.EnumProperty(s => s.IntervalType);
        schedule.Property(s => s.IntervalArgument);
        schedule.Property(s => s.Commencing);
        schedule.EnumProperty(s => s.Day);
        schedule.Property(s => s.StartHour);
        schedule.Property(s => s.StartMinute);
        schedule.Property(s => s.EndHour);
        schedule.Property(s => s.EndMinute);
        schedule.Property(s => s.TimeZone);

        var scheduleOverride = builder.EntityType<ScheduleOverride>();
        scheduleOverride.HasKey(s => s.Start);
        scheduleOverride.Property(s => s.Open);
        scheduleOverride.Property(s => s.Start);
        scheduleOverride.Property(s => s.End);

        var noticeType = builder.EnumType<VenueModels.NoticeType>();
        noticeType.Member(VenueModels.NoticeType.Information);
        noticeType.Member(VenueModels.NoticeType.Warning);
        noticeType.Member(VenueModels.NoticeType.Critical);

        var notice = builder.EntityType<Notice>();
        notice.HasKey(n => n.Id);
        notice.Property(n => n.Id).Computed();
        notice.Property(n => n.Start);
        notice.Property(n => n.End);
        notice.EnumProperty(n => n.Type);
        notice.Property(n => n.Message);

        return builder;
    }

    public static IServiceCollection AddVenuesSerializer(this IServiceCollection services) =>
        services.AddSingleton<ODataResourceSerializer, EdmComputedSerializer>()
                .AddSingleton<ODataResourceDeserializer, EdmComputedDeserializer>();

    public static ODataModelBuilder EnableLowerCamelCase(this ODataModelBuilder builder)
    {
        foreach (var type in builder.StructuralTypes)
            foreach (var prop in type.Properties)
                prop.Name = JsonNamingPolicy.CamelCase.ConvertName(prop.Name);
        return builder;
    }

}


