namespace Apha.VIR.Web.Models;

public class IsolateDispatchReportModel
{
    public string AVNumber { get; set; } = null!;
    public string Nomenclature { get; set; } = null!;
    public int NoOfAliquots { get; set; }
    public string? PassageNumber { get; set; }
    public string? Recipient { get; set; }
    public string? RecipientName { get; set; }
    public string? RecipientAddress { get; set; }
    public string? ReasonForDispatch { get; set; }
    public DateTime DispatchedDate { get; set; }
    public Guid? DispatchedBy { get; set; }
    public string DispatchedByName { get; set; } = null!;
}
