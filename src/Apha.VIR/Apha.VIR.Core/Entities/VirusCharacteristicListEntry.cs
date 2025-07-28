namespace Apha.VIR.Core.Entities;

public class VirusCharacteristicListEntry
{
    public Guid Id { get; set; }
    public Guid VirusCharacteristicId { get; set; }
    public string Name { get; set; } = null!;
    public byte[] LastModified { get; set; } = null!;
}
