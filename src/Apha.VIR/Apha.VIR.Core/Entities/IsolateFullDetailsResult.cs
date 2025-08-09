namespace Apha.VIR.Core.Entities
{
    public class IsolateFullDetailsResult
    {
        public IsolateInfo? IsolateDetails { get; set; }
        public List<IsolateViabilityInfo> IsolateViabilityDetails { get; set; } = new List<IsolateViabilityInfo>();
        public List<IsolateDispatchInfo> IsolateDispatchDetails { get; set; } = new List<IsolateDispatchInfo>();
        public List<IsolateCharacteristicInfo> IsolateCharacteristicDetails { get; set; } = new List<IsolateCharacteristicInfo>();
    }
}
