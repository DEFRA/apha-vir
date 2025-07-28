namespace Apha.VIR.Application.DTOs;

public class IsolateRelocateDTO
{
    public Guid IsolateId { get; set; }
    public Guid FreezerId { get; set; }
    public Guid TrayId { get; set; }
    public string? Well { get; set; }
    public byte[] LastModified { get; set; } = null!;
    public string UserID { get; set; } = string.Empty;
 }
