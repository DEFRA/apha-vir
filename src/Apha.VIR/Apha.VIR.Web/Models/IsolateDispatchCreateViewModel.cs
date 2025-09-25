using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.VIR.Web.Models
{
    public class IsolateDispatchCreateViewModel
    {
        public Guid? DispatchId { get; set; }
        public required Guid DispatchIsolateId { get; set; }
        public required string Avnumber { get; set; }
        public int? NoOfAliquots { get; set; } = null;
        public string? Nomenclature { get; set; }
        public required bool ValidToIssue { get; set; } = false;
        public Guid? ViabilityId { get; set; }
        public List<SelectListItem>? ViabilityList { get; set; }
        [Required(ErrorMessage = "No Of Aliquots must be entered")]
        [RegularExpression(@"^\d+$", ErrorMessage = "No of Aliquots must be a numeric value")]
        [Display(Name = "No Of Aliquots To Be Dispatched")]
        public int? NoOfAliquotsToBeDispatched { get; set; }
        [Required(ErrorMessage = "Passage Number must be entered")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Passage Number must be a numeric value")]
        [Display(Name = "Passage Number")]
        public int? PassageNumber { get; set; }
        public Guid? RecipientId { get; set; }
        public List<SelectListItem>? RecipientList { get; set; }
        public string? RecipientName { get; set; }
        public string? RecipientAddress { get; set; }
        public string? ReasonForDispatch { get; set; }
        [Required(ErrorMessage = "Dispatched Date must be entered")]
        [DataType(DataType.Date)]
        public DateTime? DispatchedDate { get; set; }
        public Guid? DispatchedById { get; set; }
        public List<SelectListItem>? DispatchedByList { get; set; }
        public byte[]? LastModified { get; set; }
        public string? RecipientLocation { get; set; }
        public bool? MaterialTransferAgreement { get; set; }
        public List<string> WarningMessages { get; set; } = new();
        public string? Source { get; set; }
        public bool IsDispatchDisabled { get; set; } = false;
        public bool IsFieldInVisible { get; set; } = false;
    }
}
