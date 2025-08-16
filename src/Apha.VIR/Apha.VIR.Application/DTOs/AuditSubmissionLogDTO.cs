namespace Apha.VIR.Application.DTOs;

public class AuditSubmissionLogDTO
{
    public Guid? LogID { get; set; }
    public string UserId { get; set; } = null!;
    //This prop populate from auth db.
    public string UserName { get; set; } = null!;
    public DateTime DateDone { get; set; }
    public Guid? SubmissionId { get; set; }
    public string AVNumber { get; set; } = null!;
    public string? SendersReferenceNumber { get; set; }
    public string? RLReferenceNumber { get; set; }
    public string? Sender { get; set; }
    public string? SenderOrganisation { get; set; }
    public string? SenderAddress { get; set; }
    public DateTime? DateSubmissionReceived { get; set; }
    public string? CPHNumber { get; set; }
    public string? Owner { get; set; }
    public string? SamplingLocationPremises { get; set; }
    public int? NumberOfSamples { get; set; }
    public string UpdateType { get; set; } = null!;
    public string? ReasonForSubmission { get; set; }
    public string? SubmittingLab { get; set; }
    public string? CountryOfOrigin { get; set; }
    public string? SubmittingCountry { get; set; }

}