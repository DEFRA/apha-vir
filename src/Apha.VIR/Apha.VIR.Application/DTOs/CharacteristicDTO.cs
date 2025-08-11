namespace Apha.VIR.Application.DTOs;

public class CharacteristicDTO
{
    public Guid CharacteristicId { get; set; }
    public Guid CharacteristicIsolateId { get; set; }
    public Guid VirusCharacteristicId { get; set; }
    public string? CharacteristicValue { get; set; }
    public byte[] LastModified { get; set; } = null!;
}