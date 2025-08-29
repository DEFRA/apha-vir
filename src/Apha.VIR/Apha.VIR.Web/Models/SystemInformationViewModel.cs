namespace Apha.VIR.Web.Models
{
    public class SystemInformationViewModel
    {
        public string? SystemName { get; set; }
        public string? DatabaseVersion { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string? Environment { get; set; }
        public bool Live { get; set; }
        public string? ReleaseNotes { get; set; }
        public string? FrameworkVersion { get; set; }
        public string? UserName { get; set; }
        public string? HostAddress { get; set; }
        public string? AuthenticationType { get; set; }
        public string? IsAuthenticated { get; set; }
    }
}
