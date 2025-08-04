namespace Apha.VIR.Core.Entities;

public class IsolateViabilityInfo
{
    public Guid IsolateViabilityIsolateId { get; set; }
    public string ViabilityStatus { get; set; } = null!;
    public DateTime DateChecked { get; set; }
    public string CheckedByName { get; set; } = null!;
}
