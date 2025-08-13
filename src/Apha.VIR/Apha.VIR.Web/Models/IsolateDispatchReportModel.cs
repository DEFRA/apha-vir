using System.ComponentModel.DataAnnotations;

namespace Apha.VIR.Web.Models;

public class IsolateDispatchReportModel
{
    [Display(Name = "AV Number")]
    public string AVNumber { get; set; } = null!;

    [Display(Name = "Nomenclature")]
    public string Nomenclature { get; set; } = null!;

    [Display(Name = "No Of Aliquots")]
    public int NoOfAliquots { get; set; }

    [Display(Name = "Passage Number")]
    public int? PassageNumber { get; set; }

    [Display(Name = "Recipient")]
    public string? Recipient { get; set; }

    [Display(Name = "Recipient Name")]
    public string? RecipientName { get; set; }

    [Display(Name = "Recipient Address")]
    public string? RecipientAddress { get; set; }

    [Display(Name = "Reason For Dispatch")]
    public string? ReasonForDispatch { get; set; }

    [Display(Name = "Dispatched Date")]
    public DateTime DispatchedDate { get; set; }

    [Display(Name = "DispatchedById")]
    public Guid? DispatchedBy { get; set; }

    [Display(Name = "Dispatched By")]
    public string DispatchedByName { get; set; } = null!;
}
