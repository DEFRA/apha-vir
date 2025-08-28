namespace Apha.VIR.Core.Entities;

public class AuditIsolateLogDetail
{
    public Guid LogId { get; set; }
    public string UserId { get; set; } = null!;
    public DateTime DateDone { get; set; }
    public string? PhylogeneticAnalysis { get; set; }
    public string? PhylogeneticFileName { get; set; }
    public string? Comment { get; set; }
    public string? Family { get; set; }
    public string? Type { get; set; }
}
