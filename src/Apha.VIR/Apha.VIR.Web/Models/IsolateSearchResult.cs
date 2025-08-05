namespace Apha.VIR.Web.Models
{
    public class IsolateSearchResult
    {
        public string Avnumber { get; set; } = null!;
        public Guid IsolateId { get; set; }
        public Guid IsolateSampleId { get; set; }
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
        public bool ValidToIssue { get; set; }
        public string? WhyNotValidToIssue { get; set; }
        public bool OriginalSampleAvailable { get; set; }
        public int? FirstViablePassageNumber { get; set; }
        public int? NoOfAliquots { get; set; }
        public Guid? Freezer { get; set; }
        public Guid? Tray { get; set; }
        public string? Well { get; set; }
        public string? IsolateNomenclature { get; set; }
        public byte[] LastModified { get; set; } = null!;
        public string? SenderReferenceNumber { get; set; }
        public Guid? IsolateViabilityId { get; set; }
        public Guid? IsolateViabilityIsolateId { get; set; }
        public Guid? Viable { get; set; }
        public DateTime? DateChecked { get; set; }
        public Guid? CheckedById { get; set; }
        public byte[]? Expr1 { get; set; }
        public string? FamilyName { get; set; }
        public string? TypeName { get; set; }
        public string? FreezerName { get; set; }
        public string? TrayName { get; set; }
        public string? Nomenclature { get; set; }
        public string? GroupSpeciesName { get; set; }
        public string? BreedName { get; set; }
        public Guid? SampleType { get; set; }
        public Guid? HostPurpose { get; set; }
        public string? CountryOfOriginName { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string? SmsreferenceNumber { get; set; }
        public string? IsolationMethodName { get; set; }
        public Guid? HostSpecies { get; set; }
        public Guid? HostBreed { get; set; }
        public string? HostPurposeName { get; set; }
        public string? SampleTypeName { get; set; }
        public Guid? CountryOfOrigin { get; set; }
        public int? IsolateNumber { get; set; }
        public int? SampleNumber { get; set; }
        public DateTime? DateCreated { get; set; }
        public string? CreatedBy { get; set; }
        public string? IsoSmsreferenceNumber { get; set; }
        public string? PhylogeneticFileName { get; set; }
    }
}
