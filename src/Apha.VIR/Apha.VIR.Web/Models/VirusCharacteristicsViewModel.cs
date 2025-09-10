using Apha.VIR.Core.Entities;

namespace Apha.VIR.Web.Models
{
    public class VirusCharacteristicsViewModel
    {
        public List<VirusCharacteristicDetails> list  { get; set; }
        public PaginationModel? Pagination { get; set; }

        public VirusCharacteristicsViewModel()
        {
            list= new List<VirusCharacteristicDetails>();   
        }
    }
}
