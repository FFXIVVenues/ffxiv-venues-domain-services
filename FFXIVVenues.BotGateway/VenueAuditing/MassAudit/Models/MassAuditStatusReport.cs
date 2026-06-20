using System.IO;

namespace FFXIVVenues.Veni.VenueAuditing.MassAudit.Models;

public class MassAuditStatusReport
{
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public Stream ContentStream { get; set; }
}