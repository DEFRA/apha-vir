namespace Apha.VIR.Application.DTOs;

public class SampleDto
{
    public Guid SampleId { get; set; }
    public Guid SampleSubmissionId { get; set; }
    public int? SampleNumber { get; set; }
    public string? SMSReferenceNumber { get; set; }
    public string? SenderReferenceNumber { get; set; }
    public Guid? SampleType { get; set; }
    public Guid? HostSpecies { get; set; }
    public Guid? HostBreed { get; set; }
    public Guid? HostPurpose { get; set; }
    public string? SamplingLocationHouse { get; set; }
    public byte[] LastModified { get; set; } = null!;
    public string? HostBreedName { get; set; }
    public string? HostSpeciesName { get; set; }
    public string? SampleTypeName { get; set; }
    public string? HostPurposeName { get; set; }
}
