using FFXIVVenues.BotGateway.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.VenueModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FFXIVVenues.BotGateway.VenueEvents.VenueSubscribing.Models;

public class VenueSubscription : IEntity
{
    public string id => GetId(UserId, VenueId);
    public ulong UserId { get; set; }
    public string VenueId { get; set; }

    public static string GetId(ulong userId, Venue venue) =>
        GetId(userId, venue.Id);

    public static string GetId(ulong userId, string venueId)
    {
        return userId + "_" + venueId;
    }
}