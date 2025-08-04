namespace Apha.VIR.Core.Entities;

public class IsolateDispatchReport
{
    public int NoOfAliquots { get; set; }
    public int PassageNumber { get; set; }
    public Guid? Recipient { get; set; }
    public string? RecipientName { get; set; }
    public string? RecipientAddress { get; set; }
    public string? ReasonForDispatch { get; set; }
    public DateTime DispatchedDate { get; set; }
    public Guid? DispatchedBy { get; set; }
    public string DispatchedByName { get; set; } = null!;
    public string RecipientStaffName { get; set; } = null!;
}
