namespace Apha.VIR.Web.Models
{
    public class VirusCharacteristicsViewModel
    {
        public List<VirusCharacteristicDetails> List  { get; set; }= new List<VirusCharacteristicDetails>();
        public PaginationModel? Pagination { get; set; }
    }
}
