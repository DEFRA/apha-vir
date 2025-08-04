namespace Apha.VIR.Core.Entities;

public class IsolateViability
{
    public Guid IsolateViabilityId { get; set; }
    public Guid IsolateViabilityIsolateId { get; set; }
    public Guid Viable { get; set; }
    public DateTime DateChecked { get; set; }
    public Guid CheckedById { get; set; }
    public byte[] LastModified { get; set; } = null!;
}
