namespace Apha.VIR.Web.Models.AuditLog
{
    public class AuditCharacteristicsLogModel
    {
        public string AvNumber { get; set; } = null!;
        public int? SampleNumber { get; set; }
        public int? IsolateNumber { get; set; }
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = string.Empty;
        public DateTime DateDone { get; set; }
        public string UpdateType { get; set; } = null!;
        public string? CharacteristicValue { get; set; }
        public string VirusCharacteristic { get; set; } = null!;
    }
}
