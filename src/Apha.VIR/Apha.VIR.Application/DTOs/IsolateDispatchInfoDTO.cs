namespace Apha.VIR.Application.DTOs;

public class IsolateDispatchInfoDTO
{
    public string Avnumber { get; set; } = null!;
    public string Nomenclature { get; set; } = null!;
    public Guid IsolateId { get; set; }

    public Guid DispatchId { get; set; }
    public int NoOfAliquots { get; set; }

    public int PassageNumber { get; set; }

    public string? Recipient { get; set; }
    public string? RecipientName { get; set; }

    public string? RecipientAddress { get; set; }

    public string? ReasonForDispatch { get; set; }

    public DateTime DispatchedDate { get; set; }

    public string DispatchedByName { get; set; } = null!;

    public Guid DispatchIsolateId { get; set; }

    public Byte[] LastModified { get; set; }
}
