namespace Apha.VIR.Application.DTOs;

public class AuditSampleLogDTO
{
    public int? SampleNumber { get; set; }
    public Guid LogId { get; set; }
    public string UserId { get; set; } = null!;
    //This prop populate from auth db.
    public string UserName { get; set; } = null!;
    public DateTime DateDone { get; set; }
    public string Avnumber { get; set; } = null!;
    public string? SmsreferenceNumber { get; set; }
    public string? SenderReferenceNumber { get; set; }
    public string? SamplingLocationHouse { get; set; }
    public string? SampleType { get; set; }
    public string? HostPurpose { get; set; }
    public string? HostBreed { get; set; }
    public string? HostSpecies { get; set; }
    public string? UpdateType { get; set; }
}