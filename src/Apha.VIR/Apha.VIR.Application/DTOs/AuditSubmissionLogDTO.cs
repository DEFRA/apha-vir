namespace Apha.VIR.Application.DTOs;

public class AuditSubmissionLogDTO
{
    public string UserId { get; set; } = null!;
    //This prop populate from auth db.
    public string UserName { get; set; } = null!;
    public DateTime DateDone { get; set; }
    public string Avnumber { get; set; } = null!;
    public string UpdateType { get; set; } = null!;
    public string? SendersReferenceNumber { get; set; }
    public DateTime? DateSubmissionReceived { get; set; }
    public string? RlreferenceNumber { get; set; }
    public Guid? SubmittingLab { get; set; }
    public string? Sender { get; set; }
    public string? SenderOrganisation { get; set; }
    public string? SenderAddress { get; set; }
    public string? Owner { get; set; }
    public string? SamplingLocationPremises { get; set; } 
    public string? Cphnumber { get; set; }
    public string? CountryOfOrigin { get; set; }
    public string? SubmittingCountry { get; set; }
    public string? ReasonForSubmission { get; set; }
    public int? NumberOfSamples { get; set; }
}
