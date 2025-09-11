using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Apha.VIR.Core.Entities;

public class IsolateRelocate
{
    public Guid? IsolateId { get; set; }
    public Guid? Freezer { get; set; }
    public Guid? Tray { get; set; }
    public string? Well { get; set; }
    public string? FreezerName { get; set; }
    public string? TrayName { get; set; }    
    public string? AVNumber { get; set; }
    public string? Nomenclature { get; set; }   
    public byte[]? LastModified { get; set; }
    [NotMapped]
    public string UserID { get; set; } = string.Empty;
    [NotMapped]
    public string UpdateType { get; set; } = string.Empty;
}
