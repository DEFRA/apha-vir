namespace Apha.VIR.Web.Models;

public class IsolateViabilityHistoryViewModel
{
    public required string Nomenclature { get; set; }
    public required IEnumerable<IsolateViabilityModel> ViabilityHistoryList { get; set; }
}
