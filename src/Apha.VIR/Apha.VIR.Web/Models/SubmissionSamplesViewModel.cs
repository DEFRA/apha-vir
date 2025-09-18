namespace Apha.VIR.Web.Models
{
    public class SubmissionSamplesViewModel
    {
        public Guid? SubmissionId { get; set; }
        public string? AVNumber {  get; set; }
        public string? SendersReferenceNumber {  get; set; }
        public string? SenderOrganisation {  get; set; }
        public string? CountryOfOriginName {  get; set; }
        public string? IsolatesGridHeader {  get; set; }
        public byte[] LastModified { get; set; } = null!;
        public bool IsLetterRequired { get; set; } = false;
        public bool ShowDeleteLink { get; set; } = true;
        public List<SubmissionSamplesModel>? Samples { get; set; }
        public List<SubmissionIsolatesModel>? Isolates {  get; set; }
    }
}
