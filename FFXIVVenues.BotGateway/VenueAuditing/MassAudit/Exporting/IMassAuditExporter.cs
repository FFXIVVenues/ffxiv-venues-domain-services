using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.Veni.VenueAuditing.MassAudit.Models;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueAuditing.MassAudit.Exporting;

public interface IMassAuditExporter
{

    MassAuditStatusSummary GetSummaryForMassAudit(MassAuditRecord massAudit, IEnumerable<Venue> allVenues,
        IList<VenueAuditRecord> audits);

    Task<MassAuditStatusReport> GetExportForMassAuditAsync(MassAuditRecord massAudit,
        IEnumerable<Venue> venues,
        IList<VenueAuditRecord> audits);

}