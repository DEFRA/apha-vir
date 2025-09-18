namespace Apha.VIR.Web.Models.VirusCharacteristic
{
    public class VirusCharacteristicsViewModel
    {
        public List<VirusCharacteristicsModel> List  { get; set; }= new List<VirusCharacteristicsModel>();
        public PaginationModel? Pagination { get; set; }
    }
}
