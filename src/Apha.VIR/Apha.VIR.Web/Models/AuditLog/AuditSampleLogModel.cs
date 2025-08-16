namespace Apha.VIR.Web.Models.AuditLog
{
    public class AuditSampleLogModel
    {
        public string AVNumber { get; set; } = null!;
        public int? SampleNumber { get; set; }
        public Guid LogId { get; set; }
        public string UserId { get; set; } = null!;
        //This prop populate from auth db.
        public string UserName { get; set; } = null!;
        public DateTime DateDone { get; set; }
        public Guid SampleId { get; set; }
        public Guid SampleSubmissionId { get; set; }
        public string? SMSReferenceNumber { get; set; }
        public string? SenderReferenceNumber { get; set; }
        public string? SamplingLocationHouse { get; set; }
        public string? SampleType { get; set; }
        public string? HostPurpose { get; set; }
        public string? HostBreed { get; set; }
        public string? HostSpecies { get; set; }
        public string? UpdateType { get; set; }
    }
}
