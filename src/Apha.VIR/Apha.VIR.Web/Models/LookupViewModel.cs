namespace Apha.VIR.Web.Models
{
    public class LookupViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string ParentName { get; set; } = null!;
        public string AlternateName { get; set; } = null!;
        public bool Active { get; set; }
        public bool Sms { get; set; }
        public string? Smscode { get; set; }
    }
}
