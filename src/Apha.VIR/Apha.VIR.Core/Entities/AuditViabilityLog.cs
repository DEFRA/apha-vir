namespace Apha.VIR.Core.Entities;

public class AuditViabilityLog
{
    public string Avnumber { get; set; } = null!;
    public int? SampleNumber { get; set; }
    public int? IsolateNumber { get; set; }
    public Guid LogId { get; set; }
    public string UserId { get; set; } = null!;
    public DateTime DateDone { get; set; }
    public string? UpdateType { get; set; }
    public Guid IsolateViabilityId { get; set; }
    public Guid IsolateViabilityIsolateId { get; set; }
    public DateTime? DateChecked { get; set; }
    public string? Viable { get; set; } = null!;
    public string? CheckedBy { get; set; } = null!;
}
