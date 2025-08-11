namespace Apha.VIR.Core.Entities;

public class IsolateCharacteristicsForSearch
{
    public string? CharacteristicValue { get; set; }
    public Guid CharacteristicIsolateId { get; set; }
    public Guid VirusCharacteristicId { get; set; }
}
