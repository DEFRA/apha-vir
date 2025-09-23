namespace Apha.VIR.Web.Models
{
    public class NavItem
    {
        public string Title { get; set; } = string.Empty;
        public string? Url { get; set; }
        public string? UpdatedUrl { get; set; }
        public string? Description { get; set; }
        public List<NavItem>? Children { get; set; }
    }
}
