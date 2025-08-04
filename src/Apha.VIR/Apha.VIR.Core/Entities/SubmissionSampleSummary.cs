namespace Apha.VIR.Core.Entities;

public class SubmissionSampleSummary
{
    public Guid SampleId { get; set; }
    public Guid SampleSubmissionId { get; set; }
    public int SampleNumber { get; set; }
    public string? SMSReferenceNumber { get; set; }
    public string? SenderReferenceNumber { get; set; }
    public string? HostBreedName { get; set; }
    public string? HostSpeciesName { get; set; }
}
