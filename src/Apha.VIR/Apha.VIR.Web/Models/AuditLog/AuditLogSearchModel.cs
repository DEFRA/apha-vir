using DocumentFormat.OpenXml.Spreadsheet;

namespace Apha.VIR.Web.Models.AuditLog
{
    public class AuditLogSearchModel
    {
        public string AVNumber { get; set; }
        public DateTime? DateTimeFrom { get; set; }
        public DateTime? DateTimeTo { get; set; }
        public string UserId { get; set; }
        
    }
}
