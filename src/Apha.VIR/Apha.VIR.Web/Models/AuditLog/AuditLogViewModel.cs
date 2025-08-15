using DocumentFormat.OpenXml.Spreadsheet;

namespace Apha.VIR.Web.Models.AuditLog
{
    public class AuditLogViewModel
    {
        public string AVNumber { get; set; }
        public DateTime? DateTimeFrom { get; set; }
        public DateTime? DateTimeTo { get; set; }
        public string SelectedUserId { get; set; }
        public string UserId { get; set; }
        public List<SubmissionLogModel> SubmissionLogs { get; set; }
        public List<SampleLogModel> SampleLogs { get; set; }
        public List<IsolateLogModel> IsolateLogs { get; set; }
        public List<CharacteristicsLogModel> CharacteristicsLogs { get; set; }
        public List<DispatchLogModel> DispatchLogs { get; set; }
        public List<IsolateViabilityLogModel> IsolateViabilityLogs { get; set; }
    }
}
