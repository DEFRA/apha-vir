namespace Apha.VIR.Application.DTOs;

public class AuditIsolateLogDTO
{
    public string AVNumber { get; set; } = null!;
    public int? SampleNumber { get; set; }
    public int? IsolateNumber { get; set; }
    public Guid LogId { get; set; }
    public string UserId { get; set; } = null!;
    //This prop populate from auth db.
    public string UserName { get; set; } = null!;
    public DateTime DateDone { get; set; }
    public string UpdateType { get; set; } = null!;
    public Guid IsolateId { get; set; }
    public Guid IsolateSampleId { get; set; }
    public int? YearOfIsolation { get; set; }
    public bool AntiserumProduced { get; set; }
    public bool AntigenProduced { get; set; }
    public bool MaterialTransferAgreement { get; set; }
    public string? MTALocation { get; set; }
    public bool? ValidToIssue { get; set; }
    public string? WhyNotValidToIssue { get; set; }
    public bool OriginalSampleAvailable { get; set; }
    public int? FirstViablePassageNumber { get; set; }
    public int? NoOfAliquots { get; set; }
    public string? Well { get; set; }
    public string? IsolateNomenclature { get; set; }
    public string? SMSReferenceNumber { get; set; }
    public string? PhylogeneticFileName { get; set; }
    public string? Tray { get; set; }
    public string? Freezer { get; set; }
    public string? Type { get; set; }
    public string? Family { get; set; }
    public string? IsolationMethod { get; set; }
}