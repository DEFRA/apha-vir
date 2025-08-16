using System.ComponentModel.DataAnnotations;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Apha.VIR.Web.Models.AuditLog
{
    public class AuditLogViewModel
    {
        [Required(ErrorMessage = "AVNumber must be entered")]
        public string AVNumber { get; set; }
        public DateTime? DateTimeFrom { get; set; }
        public DateTime? DateTimeTo { get; set; }
        public string? UserId { get; set; }
        public bool ShowErrorSummary { get; set; }
        public List<AuditSubmissionLogModel> SubmissionLogs { get; set; }
        public List<AuditSampleLogModel> SampleLogs { get; set; }
        public List<AuditIsolateLogModel> IsolateLogs { get; set; }
        public List<AuditCharacteristicsLogModel> CharacteristicsLogs { get; set; }
        public List<AuditDispatchLogModel> DispatchLogs { get; set; }
        public List<AuditIsolateViabilityLogModel> IsolateViabilityLogs { get; set; }
    }
}
