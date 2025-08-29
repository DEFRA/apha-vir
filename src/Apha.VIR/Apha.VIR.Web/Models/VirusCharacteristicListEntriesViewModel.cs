namespace Apha.VIR.Web.Models
{
    public class VirusCharacteristicListEntriesViewModel
    {
        public Guid CharacteristicId { get; set; }
        public string CharacteristicName { get; set; } = string.Empty;
        public IEnumerable<VirusCharacteristicListEntryViewModel> Entries { get; set; } = new List<VirusCharacteristicListEntryViewModel>();
    }
}
