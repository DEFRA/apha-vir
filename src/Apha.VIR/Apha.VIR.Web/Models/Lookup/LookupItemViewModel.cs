using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Models.Lookup;

public class LookupItemViewModel
{
    public Guid LookupId { get; set; } = Guid.Empty;
    public bool IsReadOnly { get; set; }=false;
    public bool ShowParent { get; set; }=false;
    public bool ShowAlternateName { get; set; } = false;
    public bool ShowSMSRelated { get; set; } = false;
    public bool? ShowErrorSummary { get; set; } =false;
    public List<SelectListItem>? LookupParentList { get; set; }
    public required LookupItemModel LookkupItem { get; set; }
}
