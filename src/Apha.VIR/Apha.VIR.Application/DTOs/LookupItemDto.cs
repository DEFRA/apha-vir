namespace Apha.VIR.Application.DTOs;

public class LookupItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public Guid? Parent { get; set; }
    public string? ParentName { get; set; }
    public string? AlternateName { get; set; }
    public bool Active { get; set; }
    public bool Sms { get; set; }
    public string? Smscode { get; set; }
    public byte[] LastModified { get; set; } = null!;
}
