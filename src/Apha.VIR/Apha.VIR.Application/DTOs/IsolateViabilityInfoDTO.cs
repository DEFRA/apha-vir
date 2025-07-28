namespace Apha.VIR.Application.DTOs;

public class IsolateViabilityInfoDTO
{
    public Guid IsolateViabilityIsolateId { get; set; }

    public string ViabilityStatus { get; set; } = null!;

    public DateTime DateChecked { get; set; }

    public string CheckedByName { get; set; } = null!;

    
}
