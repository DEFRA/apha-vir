namespace Apha.VIR.Application.DTOs;

public class LookupDTO
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Guid? Parent { get; set; }

    public string SelectCommand { get; set; } = null!;

    public string InsertCommand { get; set; } = null!;

    public string UpdateCommand { get; set; } = null!;

    public string? DeleteCommand { get; set; }

    public string InUseCommand { get; set; } = null!;

    public bool ReadOnly { get; set; }

    public bool AlternateName { get; set; }

    public bool Smsrelated { get; set; }

    public byte[] LastModified { get; set; } = null!;
}
