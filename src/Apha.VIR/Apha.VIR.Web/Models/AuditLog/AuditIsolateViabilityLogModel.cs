namespace Apha.VIR.Web.Models.AuditLog;

public class AuditIsolateViabilityLogModel
{
    public string Avnumber { get; set; } = null!;
    public int? SampleNumber { get; set; }
    public int? IsolateNumber { get; set; }
    public string UserId { get; set; } = null!;
    public string UserName { get; set; } = string.Empty;
    public DateTime DateDone { get; set; }
    public string? UpdateType { get; set; }
    public DateTime? DateChecked { get; set; }
    public string? Viable { get; set; }
    public string? CheckedBy { get; set; }
}
