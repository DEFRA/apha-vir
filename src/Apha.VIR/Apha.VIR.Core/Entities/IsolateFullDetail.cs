namespace Apha.VIR.Core.Entities
{
    public class IsolateFullDetail
    {
        public required IsolateInfo IsolateDetails { get; set; }
        public required List<IsolateViabilityInfo> IsolateViabilityDetails { get; set; }
        public required List<IsolateDispatchInfo> IsolateDispatchDetails { get; set; }
        public required List<IsolateCharacteristicInfo> IsolateCharacteristicDetails { get; set; }        
    }
}
