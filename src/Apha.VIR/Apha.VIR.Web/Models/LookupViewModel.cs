namespace Apha.VIR.Web.Models
{
    public class LookupViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid? Parent { get; set; }
        public bool ReadOnly { get; set; }
        public bool AlternateName { get; set; }
        public bool Smsrelated { get; set; }
     }
}
