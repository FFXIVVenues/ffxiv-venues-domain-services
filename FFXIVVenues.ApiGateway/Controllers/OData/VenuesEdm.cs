using FFXIVVenues.DomainData.Entities.Venues;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace FFXIVVenues.ApiGateway.Controllers.OData;

internal static class VenuesEdm
{

    public static ODataModelBuilder AddVenuesEdm(this ODataModelBuilder builder)
    {
        var venueSet = builder.EntitySet<Venue>("Venues");

        var venue = venueSet.EntityType;
        venue.HasKey(v => v.Id);
        venue.Property(v => v.Name);
        venue.Property(v => v.Banner);
        venue.Property(v => v.Added);
        venue.Property(v => v.LastModified);
        venue.CollectionProperty(v => v.Description);
        venue.ContainsOptional(v => v.Location).AutomaticallyExpand(true);
        venue.Property(v => v.Website);
        venue.Property(v => v.Discord);
        venue.Property(v => v.Hiring);
        venue.Property(v => v.Sfw);
        venue.ContainsMany(v => v.Schedule);
        venue.ContainsMany(v => v.ScheduleOverrides);
        venue.ContainsMany(v => v.Notices);
        venue.CollectionProperty(v => v.Managers);
        venue.CollectionProperty(v => v.Tags);

        var location = builder.EntityType<Location>();
        location.HasKey(l => l.Id);
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
        notice.Property(n => n.Start);
        notice.Property(n => n.End);
        notice.EnumProperty(n => n.Type);
        notice.Property(n => n.Message);

        
        return builder;
    }

    public static IServiceCollection AddVenuesSerializer(this IServiceCollection services, string bannerUriTemplate)
    {
        

        services.AddSingleton<ODataResourceSerializer>(provider => 
            new VenueSerializer(provider.GetRequiredService<IODataSerializerProvider>(), bannerUriTemplate));
        return services;
    }
}

internal class VenueSerializer(IODataSerializerProvider provider, string bannerUriTemplate)
    : ODataResourceSerializer(provider)
{
    public override ODataProperty CreateStructuralProperty(IEdmStructuralProperty property, ResourceContext context)
    {
        if (property.Name != "Banner" || property.DeclaringType.FullTypeName() != typeof(Venue).FullName)
            return base.CreateStructuralProperty(property, context);

        var bannerKey = context.GetPropertyValue("Banner") as string;
        var venueId = context.GetPropertyValue("Id") as string;
        var uri = bannerKey is null ? null
            : bannerUriTemplate.Replace("{venueId}", venueId).Replace("{bannerKey}", bannerKey);
        return new ODataProperty { Name = property.Name, Value = uri };
    }
}