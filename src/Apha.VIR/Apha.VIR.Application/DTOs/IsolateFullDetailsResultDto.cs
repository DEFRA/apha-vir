using Apha.VIR.Core.Entities;

namespace Apha.VIR.Application.DTOs
{
    public class IsolateFullDetailsResultDto
    {
        public IsolateInfoDTO IsolateDetails { get; set; }
        public List<IsolateViabilityInfoDTO> IsolateViabilityDetails { get; set; }
        public List<IsolateDispatchInfoDTO> IsolateDispatchDetails { get; set; }
        public List<IsolateCharacteristicInfoDTO> IsolateCharacteristicDetails { get; set; }
    }
}
