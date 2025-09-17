namespace Apha.VIR.Application.DTOs;

public class IsolateViabilityInfoDto
{
    public Guid IsolateViabilityId { get; set; }
    public Guid IsolateViabilityIsolateId { get; set; }
    public Guid Viable { get; set; }
    public string ViabilityStatus { get; set; } = null!;
    public DateTime DateChecked { get; set; }
    public Guid CheckedById { get; set; }
    public byte[] LastModified { get; set; } = null!;
    public string CheckedByName { get; set; } = null!;
    public string ViableName { get; set; } = null!;
    public string? Nomenclature { get; set; } = null!;
    public string? AVNumber { get; set; } = null!;
}