namespace Apha.VIR.Application.DTOs;

public class IsolateDTO
{
    public Guid IsolateId { get; set; }
    public Guid IsolateSampleId { get; set; }
    public int? IsolateNumber { get; set; }
    public Guid Family { get; set; }
    public Guid Type { get; set; }
    public int? YearOfIsolation { get; set; }
    public bool IsMixedIsolate { get; set; }
    public Guid? IsolationMethod { get; set; }
    public bool AntiserumProduced { get; set; }
    public bool AntigenProduced { get; set; }
    public string? PhylogeneticAnalysis { get; set; }
    public bool MaterialTransferAgreement { get; set; }
    public string? Mtalocation { get; set; }
    public string? Comment { get; set; }
    public bool? ValidToIssue { get; set; }
    public string? WhyNotValidToIssue { get; set; }
    public bool OriginalSampleAvailable { get; set; }
    public int? FirstViablePassageNumber { get; set; }
    public int? NoOfAliquots { get; set; }
    public Guid? Freezer { get; set; }
    public Guid? Tray { get; set; }
    public string? Well { get; set; }
    public string? IsolateNomenclature { get; set; }
    public byte[] LastModified { get; set; } = null!;
    public DateTime? DateCreated { get; set; }
    public string? CreatedBy { get; set; }
    public string? SmsreferenceNumber { get; set; }
    public string? PhylogeneticFileName { get; set; }
}
