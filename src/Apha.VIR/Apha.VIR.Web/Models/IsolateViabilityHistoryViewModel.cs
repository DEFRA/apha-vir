namespace Apha.VIR.Web.Models;

namespace Apha.VIR.Web.Models
{
    public class IsolateViabilityHistoryViewModel
    {
        public Guid? IsolateId { get; set; }
        public string Nomenclature { get; set; }

        public IEnumerable<IsolateViabilityModel> ViabilityHistoryList { get; set; }

    }
public class IsolateViabilityHistoryViewModel
{
    public required string Nomenclature { get; set; }
    public required IEnumerable<IsolateViabilityModel> ViabilityHistoryList { get; set; }
}
