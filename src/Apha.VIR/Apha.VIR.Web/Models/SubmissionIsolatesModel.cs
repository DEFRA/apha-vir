namespace Apha.VIR.Web.Models
{
    public class SubmissionIsolatesModel
    {
        public Guid IsolateId { get; set; }
        public Guid IsolateSampleId { get; set; }
        public int? SampleNumber { get; set; }
        public string? IsoSMSReferenceNumber { get; set; }
        public string? SenderReferenceNumber { get; set; }
        public string? FamilyName { get; set; }
        public Guid Type { get; set; }
        public string? TypeName { get; set; }
        public int? YearOfIsolation { get; set; }
        public string? Characteristics { get; set; }
        public string? Nomenclature { get; set; }
        public bool IsUniqueNomenclature { get; set; } = false;
        public byte[] LastModified { get; set; } = null!;
        public bool IsDeleteEnabled { get; set; } = false;
        public bool IsDispatchEnabled { get; set; } = false; 
    }
}
