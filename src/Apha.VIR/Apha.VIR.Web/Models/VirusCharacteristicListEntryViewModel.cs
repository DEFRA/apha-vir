namespace Apha.VIR.Web.Models
{
    public class VirusCharacteristicListEntryViewModel
    {
        public Guid Id { get; set; }
        public Guid VirusCharacteristicId { get; set; }
        public string Name { get; set; } = string.Empty;
        public byte[] LastModified { get; set; } = Array.Empty<byte>();
    }
}
