namespace Apha.VIR.Core.Entities;

public  class Sample
{
    public Guid SampleId { get; set; }
    public Guid SampleSubmissionId { get; set; }
    public int? SampleNumber { get; set; }
    public string? SmsreferenceNumber { get; set; }
    public string? SenderReferenceNumber { get; set; }
    public Guid? SampleType { get; set; }
    public Guid? HostSpecies { get; set; }
    public Guid? HostBreed { get; set; }
    public Guid? HostPurpose { get; set; }
    public string? SamplingLocationHouse { get; set; }
    public byte[] LastModified { get; set; } = null!;
}
