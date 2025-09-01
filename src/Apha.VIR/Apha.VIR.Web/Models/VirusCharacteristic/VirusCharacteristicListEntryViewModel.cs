namespace Apha.VIR.Web.Models.VirusCharacteristic
{
    public class VirusCharacteristicListEntryViewModel
    {
        public List<VirusCharacteristicListEntryModel> VirusCharacteristics { get; set; } = new List<VirusCharacteristicListEntryModel>();
        public PaginationModel? Pagination { get; set; }
    }
}
