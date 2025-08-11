namespace Apha.VIR.Core.Entities;

public partial class Staff
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public Guid? Parent { get; set; }
    public bool Active { get; set; }
    public bool Sms { get; set; }
    public string? Smscode { get; set; }
    public byte[] LastModified { get; set; } = null!;
}
