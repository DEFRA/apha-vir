namespace Apha.VIR.Application.DTOs;

public class IsolateCharacteristicInfoDTO
{
    public string? CharacteristicValue { get; set; }

    public string CharacteristicName { get; set; } = null!;

    public Guid IsolateId { get; set; }

    public string? CharacteristicPrefix { get; set; }

    public Guid CharacteristicId { get; set; }
}
