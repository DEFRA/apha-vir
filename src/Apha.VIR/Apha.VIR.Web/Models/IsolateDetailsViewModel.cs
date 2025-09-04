using System.ComponentModel.DataAnnotations;

namespace Apha.VIR.Web.Models
{
    public class IsolateDetailsViewModel
    {
        public IsolateDetails? IsolateDetails { get; set; }
        public required List<IsolateViabilityCheckInfo> IsolateViabilityDetails { get; set; }
        public required List<IsolateDispatchInfo> IsolateDispatchDetails { get; set; }
        public required List<IsolateCharacteristicInfo> IsolateCharacteristicDetails { get; set; }
    }
}
