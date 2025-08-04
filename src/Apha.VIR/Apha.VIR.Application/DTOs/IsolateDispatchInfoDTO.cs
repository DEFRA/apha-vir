namespace Apha.VIR.Application.DTOs;

public class IsolateDispatchInfoDTO
{
    public int NoOfAliquots { get; set; }

    public int PassageNumber { get; set; }

    public string? RecipientName { get; set; }

    public string? RecipientAddress { get; set; }

    public string? ReasonForDispatch { get; set; }

    public DateTime DispatchedDate { get; set; }

    public string DispatchedByName { get; set; } = null!;

    public Guid DispatchIsolateId { get; set; }
}
