namespace Apha.VIR.Web.Models
{
    public class BreadcrumbEntry
    {
        public string Url { get; set; } = string.Empty;
        public Dictionary<string, string> Parameters { get; set; } = new();
    }
}
