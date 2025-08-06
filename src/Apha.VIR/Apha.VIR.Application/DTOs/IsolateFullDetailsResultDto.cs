using Apha.VIR.Core.Entities;

namespace Apha.VIR.Application.DTOs
{
    public class IsolateFullDetailsResultDto
    {
        public IsolateInfoDTO? IsolateDetails { get; set; }
        public List<IsolateViabilityInfoDTO> IsolateViabilityDetails { get; set; } = new List<IsolateViabilityInfoDTO>();
        public List<IsolateDispatchInfoDTO> IsolateDispatchDetails { get; set; } = new List<IsolateDispatchInfoDTO>();
        public List<IsolateCharacteristicInfoDTO> IsolateCharacteristicDetails { get; set; } = new List<IsolateCharacteristicInfoDTO>();
    }
}
