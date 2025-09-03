namespace Apha.VIR.Web.Models.VirusCharacteristic
{
    public class VirusCharacteristicListEntriesViewModel
    {
        public Guid CharacteristicId { get; set; }
        public string CharacteristicName { get; set; } = string.Empty;
        public VirusCharacteristicListEntryViewModel Entries { get; set; } = new VirusCharacteristicListEntryViewModel();
    }
}
