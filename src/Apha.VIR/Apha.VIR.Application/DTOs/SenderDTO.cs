namespace Apha.VIR.Application.DTOs;

public class SenderDTO
{
    public Guid SenderId { get; set; }
    public string? SenderName { get; set; }
    public string? SenderOrganisation { get; set; }
    public string? SenderAddress { get; set; }
    public Guid? Country { get; set; }
    public string? CountryName { get; set; }
}
