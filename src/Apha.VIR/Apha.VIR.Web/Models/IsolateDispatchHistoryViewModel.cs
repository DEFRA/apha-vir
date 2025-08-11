namespace Apha.VIR.Web.Models;

public class IsolateDispatchHistoryViewModel
{
    public Guid? IsolateId { get; set; }
    public string? Nomenclature { get; set; }

    public IEnumerable<IsolateDispatchHistory>? DispatchHistoryRecords { get; set; }
}
