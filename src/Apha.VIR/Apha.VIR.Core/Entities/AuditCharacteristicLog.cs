namespace Apha.VIR.Core.Entities;

public class AuditCharacteristicLog
{
    public string Avnumber { get; set; } = null!;
    public int? SampleNumber { get; set; }
    public int? IsolateNumber { get; set; }
    public Guid LogId { get; set; }
    public string UserId { get; set; } = null!;
   // public string? UserName { get; set; } = null!;
    public DateTime DateDone { get; set; }
    public string UpdateType { get; set; } = null!;
    public Guid CharacteristicId { get; set; }
    public Guid CharacteristicIsolateId { get; set; }
    public string? CharacteristicValue { get; set; }
    public string VirusCharacteristic { get; set; } = null!;
}
