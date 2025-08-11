namespace Apha.VIR.Application.DTOs;

public class IsolateFullDetailDTO
{
    public IsolateInfoDTO? IsolateDetails { get; set; }
    public required List<IsolateViabilityInfoDTO> IsolateViabilityDetails { get; set; }
    public required List<IsolateDispatchInfoDTO> IsolateDispatchDetails { get; set; }
    public required List<IsolateCharacteristicInfoDTO> IsolateCharacteristicDetails { get; set; }
}