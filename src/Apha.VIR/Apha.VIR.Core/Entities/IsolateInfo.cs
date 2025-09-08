namespace Apha.VIR.Core.Entities;

public class IsolateInfo
{
    public Guid IsolateId { get; set; }
    public Guid IsolateSampleId { get; set; }
    public string? AvNumber { get; set; } = null!;
    public string? FamilyName { get; set; }
    public string? TypeName { get; set; }
    public string? GroupSpeciesName { get; set; }
    public string? BreedName { get; set; }
    public string? CountryOfOriginName { get; set; }
    public int? YearOfIsolation { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string? FreezerName { get; set; }
    public string? TrayName { get; set; }
    public string? Well { get; set; }
    public bool MaterialTransferAgreement { get; set; }
    public int NoOfAliquots { get; set; } = 0;
    public string? SenderReferenceNumber { get; set; }
    public string? IsolationMethodName { get; set; }
    public bool AntiserumProduced { get; set; }
    public bool AntigenProduced { get; set; }
    public string? PhylogeneticAnalysis { get; set; }
    public string? PhylogeneticFileName { get; set; }
    public string? Mtalocation { get; set; }
    public string? Comment { get; set; }
    public bool? ValidToIssue { get; set; }
    public string? WhyNotValidToIssue { get; set; }
    public bool OriginalSampleAvailable { get; set; }
    public int? FirstViablePassageNumber { get; set; }
    public bool IsMixedIsolate { get; set; }
    public string? Nomenclature { get; set; }
    public string? SmsreferenceNumber { get; set; }
    public string? HostPurposeName { get; set; }
    public string? SampleTypeName { get; set; }
}
