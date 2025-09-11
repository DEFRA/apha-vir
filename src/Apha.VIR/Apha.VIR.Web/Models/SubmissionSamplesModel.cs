namespace Apha.VIR.Web.Models
{
    public class SubmissionSamplesModel
    {
        public Guid? SampleId { get; set; }        
        public Guid? SampleSubmissionID { get; set; }
        public string? AVNumber { get; set; }
        public string? SampleNumber { get; set; }
        public string? SMSReferenceNumber { get; set; }
        public string? SenderReferenceNumber { get; set; }
        public string? HostBreedName { get; set; }
        public string? HostSpeciesName { get; set; }
        public string? SampleTypeName { get; set; }
        public byte[] LastModified { get; set; } = null!;
        public bool IsDetection { get; set; }
        public bool IsDeleteEnabled { get; set; } = false;
    }
}
