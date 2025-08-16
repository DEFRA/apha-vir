namespace Apha.VIR.Web.Models.AuditLog
{
    public class AuditIsolateViabilityLogModel
    {
        public string Avnumber { get; set; } = null!;
        public int? SampleNumber { get; set; }
        public int? IsolateNumber { get; set; }
        //This prop populate from auth db.
        public string UserName { get; set; } = null!;
        public Guid LogId { get; set; }
        public string UserId { get; set; } = null!;
        public DateTime DateDone { get; set; }
        public string UpdateType { get; set; } = null!;
        public Guid IsolateViabilityId { get; set; }
        public Guid IsolateViabilityIsolateId { get; set; }
        public DateTime? DateChecked { get; set; }
        public string Viable { get; set; } = null!;
        public string CheckedBy { get; set; } = null!;
    }
}
