namespace Apha.VIR.Web.Models.AuditLog;

public class AuditIsolateLogDetailsViewModel
{
    public string AVNumber { get; set; } = string.Empty;
    public string? Family { get; set; }
    public string? Type { get; set; }
    public DateTime DateOfChange { get; set; }
    public string User { get; set; } = string.Empty;
    public string? PhylogeneticAnalysis { get; set; }
    public string? PhylogeneticFileName { get; set; }
    public string? Comment { get; set; }
}
