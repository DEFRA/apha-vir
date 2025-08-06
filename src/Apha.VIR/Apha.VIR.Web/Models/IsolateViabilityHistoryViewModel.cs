using System.ComponentModel.DataAnnotations;

namespace Apha.VIR.Web.Models
{
    public class IsolateViabilityHistoryViewModel
    {
        public string Nomenclature { get; set; }

        public IEnumerable<IsolateViabilityModel> ViabilityHistoryList { get; set; }

    }
}
