namespace Apha.VIR.Application.DTOs;

public class SubmissionDTO
{
    public Guid SubmissionId { get; set; }
    public string Avnumber { get; set; } = null!;
    public string? SendersReferenceNumber { get; set; }
    public string? RlreferenceNumber { get; set; }
    public Guid? SubmittingLab { get; set; }
    public string? Sender { get; set; }
    public string? SenderOrganisation { get; set; }
    public string? SenderAddress { get; set; }
    public Guid? CountryOfOrigin { get; set; }
    public Guid? SubmittingCountry { get; set; }
    public Guid? ReasonForSubmission { get; set; }
    public DateTime? DateSubmissionReceived { get; set; }
    public string? Cphnumber { get; set; }
    public string? Owner { get; set; }
    public string? SamplingLocationPremises { get; set; }
    public int? NumberOfSamples { get; set; }
    public byte[] LastModified { get; set; } = null!;
    public string? CountryOfOriginName { get; set; }
}
