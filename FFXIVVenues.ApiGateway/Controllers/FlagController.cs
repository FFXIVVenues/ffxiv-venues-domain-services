using FFXIVVenues.DomainData.Entities.Flags;
using FFXIVVenues.FlagService.Client;
using Microsoft.AspNetCore.Mvc;

namespace FFXIVVenues.ApiGateway.Controllers;

/// <summary>
/// Venue flagging endpoints.
/// </summary>
/// <param name="id">The Id of the venue to flag.</param>
[ApiController]
[ApiVersion("1.0")]
[Route("v{apiVersion:ApiVersion}/venue/{id}/flag")]
public class FlagController(IFlagServiceClient flagServiceClient): ControllerBase
{
    /// <summary>
    /// Flag a venue.
    /// </summary>
    /// <remarks>
    /// Flags a venue for inappropriate content, incorrect information, or empty venue.
    /// If the venue has been flagged already by recently by the address, subsequent flags will be ignored.  
    /// </remarks>
    /// <param name="flag">The flag information containing venue ID, category, and description.</param>
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> with the flag data if successful.
    /// </returns>
    [HttpPut]
    public ActionResult Flag([FromRoute] string id, [FromBody] FlagDto flag)
    {
        var ip = this.HttpContext.Connection.RemoteIpAddress;
        if (ip == null) // Will be hashed, but non-the-less needed
            return Unauthorized();
        flagServiceClient.SendFlagAsync(id, flag.Category, flag.Description, ip);
        return Accepted();
    }
}

/// <summary>
/// Data transfer object for venue flagging.
/// </summary>
/// <param name="Category">The type of flag.</param>
/// <param name="Description">Additional description for the flag.</param>
public record FlagDto(FlagCategory Category, string Description);
