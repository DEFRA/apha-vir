namespace Apha.VIR.Web.Models
{
    public class IsolateCharacteristicInfo
    {
        public string? CharacteristicValue { get; set; }

        public string CharacteristicName { get; set; } = null!;

        public Guid IsolateId { get; set; }

        public string? CharacteristicPrefix { get; set; }

        public Guid CharacteristicId { get; set; }
    }
}
