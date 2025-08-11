namespace Apha.VIR.Web.Models;

public class IsolateViabilityHistoryViewModel
{
    public Guid? IsolateId { get; set; }
    public required string Nomenclature { get; set; }

    public required IEnumerable<IsolateViabilityModel> ViabilityHistoryList { get; set; }

}

