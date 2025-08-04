namespace Apha.VIR.Core.Entities;

public class AuditDispatchLog
{
    public string Avnumber { get; set; } = null!;
    public int? SampleNumber { get; set; }
    public int? IsolateNumber { get; set; }
    //This prop populate from auth db.
    public string UserName { get; set; } = null!;
    public Guid LogId { get; set; }
    public string UserId { get; set; } = null!;
    public DateTime DateDone { get; set; }
    public string UpdateType { get; set; } = null!;
    public Guid DispatchId { get; set; }
    public Guid DispatchIsolateId { get; set; }
    public int NoOfAliquots { get; set; }
    public int PassageNumber { get; set; }
    public string? RecipientName { get; set; }
    public string? RecipientAddress { get; set; }
    public string? ReasonForDispatch { get; set; }
    public DateTime DispatchedDate { get; set; }
    public string? DispatchedBy { get; set; }
}
