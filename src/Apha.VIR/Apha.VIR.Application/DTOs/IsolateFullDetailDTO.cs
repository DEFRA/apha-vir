namespace Apha.VIR.Application.DTOs;

public class IsolateFullDetailDto
{
    public IsolateInfoDto? IsolateDetails { get; set; }
    public required List<IsolateViabilityInfoDto> IsolateViabilityDetails { get; set; }
    public required List<IsolateDispatchInfoDto> IsolateDispatchDetails { get; set; }
    public required List<IsolateCharacteristicInfoDto> IsolateCharacteristicDetails { get; set; }
}