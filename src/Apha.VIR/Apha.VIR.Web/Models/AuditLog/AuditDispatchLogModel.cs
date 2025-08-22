namespace Apha.VIR.Web.Models.AuditLog;
public class AuditDispatchLogModel
{
    public string Avnumber { get; set; } = null!;
    public int? SampleNumber { get; set; }
    public int? IsolateNumber { get; set; }
    public string UserId { get; set; } = null!;
    public string UserName { get; set; } = string.Empty;
    public DateTime DateDone { get; set; }
    public string? UpdateType { get; set; }
    public int NoOfAliquots { get; set; }
    public int PassageNumber { get; set; }
    public string? RecipientName { get; set; }
    public string? RecipientAddress { get; set; }
    public string? ReasonForDispatch { get; set; }
    public DateTime DispatchedDate { get; set; }
    public string? DispatchedBy { get; set; }
}
