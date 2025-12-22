using FFXIVVenues.DomainData.Entities.Flags;
using FFXIVVenues.FlagService.Client;
using Microsoft.AspNetCore.Mvc;

namespace FFXIVVenues.ApiGateway.Controllers;

/// <summary>
/// Venue flagging endpoints.
/// </summary>
[ApiController]
public class FlagController(IFlagServiceClient flagServiceClient): ControllerBase
{
    /// <summary>
    /// Flags a venue for inappropriate content, incorrect information, or empty venue.
    /// </summary>
    /// <remarks>
    /// If the venue has been flagged already by recently by the address, subsequent flags will be ignored.  
    /// </remarks>
    /// <param name="flag">The flag information containing venue ID, category, and description.</param>
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> with the flag data if successful.
    /// </returns>
    [HttpPut("venue/{id}/flag")]
    public ActionResult Flag([FromBody] FlagDto flag)
    {
        var ip = this.HttpContext.Connection.RemoteIpAddress;
        if (ip == null) // Will be hashed, but non-the-less needed
            return Unauthorized();
        flagServiceClient.SendFlagAsync(flag.VenueId, flag.Category, flag.Description, ip);
        return Ok(flag);
    }
}

/// <summary>
/// Data transfer object for venue flagging.
/// </summary>
/// <param name="VenueId">The ID of the venue to be flagged.</param>
/// <param name="Category">The type of flag.</param>
/// <param name="Description">Additional description for the flag.</param>
public record FlagDto(string VenueId, FlagCategory Category, string Description);
