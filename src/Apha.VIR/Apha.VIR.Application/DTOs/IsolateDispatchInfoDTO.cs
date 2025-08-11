namespace Apha.VIR.Application.DTOs;

public class IsolateDispatchInfoDTO
{
    public Guid DispatchId { get; set; }
    public Guid IsolateId { get; set; }
    public Guid DispatchIsolateId { get; set; }
    public string Avnumber { get; set; } = null!;
    public string Nomenclature { get; set; } = null!;
    public bool ValidToIssue { get; set; }
    public Guid? ViabilityId { get; set; }
    public  string? ViabilityName { get; set; }
    public int NoOfAliquotsToBeDispatched { get; set; }
    public int NoOfAliquots { get; set; }
    public int PassageNumber { get; set; }
    public Guid? RecipientId { get; set; }
    public string? Recipient { get; set; }
    public string? RecipientName { get; set; }
    public string? RecipientAddress { get; set; }
    public string? ReasonForDispatch { get; set; }
    public DateTime DispatchedDate { get; set; }
    public string DispatchedByName { get; set; } = null!;
    public Guid? DispatchedById { get; set; } = null!;
    public Byte[] LastModified { get; set; } = Array.Empty<byte>();
}
