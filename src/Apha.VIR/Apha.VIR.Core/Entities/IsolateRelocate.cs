namespace Apha.VIR.Core.Entities;

public class IsolateRelocate
{
    public Guid IsolateId { get; set; }
    public Guid FreezerId { get; set; }
    public Guid TrayId { get; set; }
    public string? Well { get; set; }
    public byte[] LastModified { get; set; } = null!;
    public string UserID { get; set; } = string.Empty;
}
