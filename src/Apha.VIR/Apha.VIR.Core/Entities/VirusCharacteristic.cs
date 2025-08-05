namespace Apha.VIR.Core.Entities;

public class VirusCharacteristic
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public Guid CharacteristicType { get; set; }
    public bool NumericSort { get; set; }
    public bool DisplayOnSearch { get; set; }
    public string? Prefix { get; set; }
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public int? DecimalPlaces { get; set; }
    public int? Length { get; set; }
    public int? CharacteristicIndex { get; set; }
    public byte[] LastModified { get; set; } = null!;
    public string VirusCharacteristicTypeName { get; set; } = null!;
}
