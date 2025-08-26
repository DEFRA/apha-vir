using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Models.Lookup;

public class LookupItemViewModel
{
    public Guid LookupId { get; set; }
    [Required]
    public bool IsReadOnly { get; set; }
    [Required]
    public bool ShowParent { get; set; }
    [Required]
    public bool ShowAlternateName { get; set; }
    [Required]
    public bool ShowSMSRelated { get; set; }
    [Required]
    public bool ShowErrorSummary { get; set; } 
    public List<SelectListItem>? LookupParentList { get; set; }
    public required LookupItemModel LookkupItem { get; set; }
}
