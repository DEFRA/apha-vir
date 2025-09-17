namespace Apha.VIR.Application.DTOs;

public class IsolateDispatchReportDto
{
    public string? AVNumber { get; set; } = null!;
    public string? Nomenclature { get; set; } = null!;
    public string? IsolateNomenclature { get; set; } = null!;
    public int NoOfAliquots { get; set; }
    public int PassageNumber { get; set; }
    public Guid? RecipientId { get; set; }
    public string? Recipient { get; set; }
    public string? RecipientName { get; set; }
    public string? RecipientAddress { get; set; }
    public string? ReasonForDispatch { get; set; }
    public DateTime DispatchedDate { get; set; }
    public Guid? DispatchedBy { get; set; }
    public string DispatchedByName { get; set; } = null!;
    public string RecipientIDName { get; set; } = null!;
}