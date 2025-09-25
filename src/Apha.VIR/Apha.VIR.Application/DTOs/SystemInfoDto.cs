namespace Apha.VIR.Application.DTOs;

public class SystemInfoDto
{
    public Guid Id { get; set; }
    public string SystemName { get; set; } = null!;
    public string DatabaseVersion { get; set; } = null!;
    public DateTime ReleaseDate { get; set; }
    public string Environment { get; set; } = null!;
    public bool Live { get; set; }
    public string? ReleaseNotes { get; set; }
}
