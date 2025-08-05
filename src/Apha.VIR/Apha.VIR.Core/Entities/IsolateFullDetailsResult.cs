namespace Apha.VIR.Core.Entities
{
    public class IsolateFullDetailsResult
    {
        public IsolateInfo? IsolateDetails { get; set; }
        public List<IsolateViabilityInfo>? IsolateViabilityDetails { get; set; }
        public List<IsolateDispatchInfo>? IsolateDispatchDetails { get; set; }
        public List<IsolateCharacteristicInfo>? IsolateCharacteristicDetails { get; set; }        
    }
}
