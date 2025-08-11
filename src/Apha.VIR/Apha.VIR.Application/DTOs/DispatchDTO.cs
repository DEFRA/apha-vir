namespace Apha.VIR.Application.DTOs;

public class DispatchDTO
{
    public Guid DispatchId { get; set; }
    public Guid DispatchIsolateId { get; set; }
    public int NoOfAliquots { get; set; }
    public int PassageNumber { get; set; }
    public Guid? Recipient { get; set; }
    public string? RecipientName { get; set; }
    public string? RecipientAddress { get; set; }
    public string? ReasonForDispatch { get; set; }
    public DateTime DispatchedDate { get; set; }
    public Guid? DispatchedBy { get; set; }
    public byte[] LastModified { get; set; } = null!;
}