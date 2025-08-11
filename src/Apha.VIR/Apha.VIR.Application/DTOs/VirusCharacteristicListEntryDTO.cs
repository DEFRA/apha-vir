namespace Apha.VIR.Application.DTOs;

public class VirusCharacteristicListEntryDTO
{
    public Guid Id { get; set; }
    public Guid VirusCharacteristicId { get; set; }
    public string Name { get; set; } = null!;
    public byte[] LastModified { get; set; } = null!;
}
