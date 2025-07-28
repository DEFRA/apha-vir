namespace Apha.VIR.Core.Entities;

public class Characteristic
{
    public Guid CharacteristicId { get; set; }
    public Guid CharacteristicIsolateId { get; set; }
    public Guid VirusCharacteristicId { get; set; }
    public string? CharacteristicValue { get; set; }
    public byte[] LastModified { get; set; } = null!;
}
