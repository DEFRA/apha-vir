using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Models.Lookup;

public class LookupItemViewModel
{
    public Guid LookupId { get; set; }
    public bool IsReadOnly { get; set; }
    public bool ShowParent { get; set; }
    public bool ShowAlternateName { get; set; }
    public bool ShowSMSRelated { get; set; }
    public bool ShowErrorSummary { get; set; } 
    public List<SelectListItem>? LookupParentList { get; set; }
    public required LookupItemModel LookkupItem { get; set; }
}
