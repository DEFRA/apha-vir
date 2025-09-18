using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Models
{
    public class IsolateDispatchEditViewModel
    {
        public Guid? DispatchId { get; set; }
        public Guid? DispatchIsolateId { get; set; }

        [Display(Name = "AV Number")]
        public string? Avnumber { get; set; }

        [Display(Name = "Number of Aliquots")]
        public int NoOfAliquots { get; set; }

        [Display(Name = "Nomenclature")]
        public string? Nomenclature { get; set; }

        [Required]
        [Display(Name = "ValidToIssue")]
        public bool ValidToIssue { get; set; }

        [Display(Name = "Viability")]
        public Guid? ViabilityId { get; set; }

        public List<SelectListItem>? ViabilityList { get; set; }

        [Required(ErrorMessage = "No Of Aliquots must be entered")]
        [RegularExpression(@"^\d+$", ErrorMessage = "No of Aliquots must be a numeric value")]
        [Display(Name = "Number of Aliquots")]
        public int? NoOfAliquotsToBeDispatched { get; set; }

        [Required(ErrorMessage = "Passage Number must be entered")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Passage Number must be a numeric value")]
        [Display(Name = "Passage Number")]
        public int? PassageNumber { get; set; }

        [Display(Name = "Recipient")]
        public Guid? RecipientId { get; set; }

        public List<SelectListItem>? RecipientList { get; set; }

        [Display(Name = "Recipient Name")]
        public string? RecipientName { get; set; }

        [Display(Name = "Recipient Address")]
        public string? RecipientAddress { get; set; }
        [Display(Name = "Reason for Dispatch")]
        public string? ReasonForDispatch { get; set; }
        [Required(ErrorMessage = "Dispatched Date must be entered")]
        [Display(Name = "Dispatched Date")]
        [DataType(DataType.Date)]
        public DateTime? DispatchedDate { get; set; }
        [Display(Name = "Dispatched By")]
        public Guid? DispatchedById { get; set; }
        public List<SelectListItem>? DispatchedByList { get; set; }
        [Display(Name = "Last Modified")]
        public byte[]? LastModified { get; set; }
        public string? RecipientLocation { get; set; }        
    }
}
