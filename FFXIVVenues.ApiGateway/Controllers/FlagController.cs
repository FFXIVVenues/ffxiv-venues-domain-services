using FFXIVVenues.DomainData.Entities.Flags;
using FFXIVVenues.FlagService.Client;
using Microsoft.AspNetCore.Mvc;

namespace FFXIVVenues.ApiGateway.Controllers;

[ApiController]
public class FlagController(IFlagServiceClient flagServiceClient): ControllerBase
{
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

public record FlagDto(string VenueId, FlagCategory Category, string Description);
