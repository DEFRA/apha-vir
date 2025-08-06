namespace Apha.VIR.Core.Entities
{
    public class LookupItemByParent
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;        
        public string? AlternateName { get; set; } = null!;
        public bool Active { get; set; }
        public bool Sms { get; set; }
        public string? Smscode { get; set; }
    }
}
