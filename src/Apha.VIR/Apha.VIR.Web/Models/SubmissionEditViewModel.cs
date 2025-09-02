using Apha.VIR.Web.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.VIR.Web.Models
{
    public class SubmissionEditViewModel
    {
        public Guid? SubmissionId { get; set; }

        [Required(ErrorMessage = "AV Number is required")]
        [StringLength(256, ErrorMessage = "AV Number may not be more than 256 characters long")]
        public string? AVNumber { get; set; }
        public string? RLReferenceNumber { get; set; }
        public string? SubmittingLab { get; set; }

        [RegularExpression(@"[\da-zA-Z'.,\-\\ /()#]*", ErrorMessage = "Letters and numbers only")]
        public string? SendersReferenceNumber { get; set; }
        public string? SubmittingCountry { get; set; }

        [MaxLength(50)]
        public string? Sender { get; set; }

        [MaxLength(200)]
        public string? SenderOrganisation { get; set; }

        [MaxLength(500)]
        public string? SenderAddress { get; set; }
        public string? CountryOfOrigin { get; set; }
        public string? ReasonForSubmission { get; set; }

        [DataType(DataType.Date)]
        //[RegularExpression(@"^(0[1-9]|[12][0-9]|3[01])/(0[1-9]|1[0-2])/\d{4}$", ErrorMessage = "Not a valid date (dd/mm/yyyy).")]        
        [DateRange("01/01/1900", ErrorMessage = "Date must be between {0} and {1}.")]
        public DateTime? DateSubmissionReceived { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Must be a positive number.")]
        public int? NumberOfSamples { get; set; }

        [RegularExpression(@"\d{2}/\d{3}/\d{4}(/\d{2}|)", ErrorMessage = "Format is 00/000/0000 or 00/000/0000/00")]
        public string? CPHNumber { get; set; }
        public string? Owner { get; set; }
        public string? SamplingLocationPremises { get; set; }
        public byte[] LastModified { get; set; } = null!;
        public List<SelectListItem>? SubmittingLabList { get; set; }
        public List<SelectListItem>? SubmissionReasonList { get; set; }
        public List<SelectListItem>? CountryList { get; set; }
        public List<SubmissionSenderViewModel>? Senders { get; set; }
        public List<SubmissionSenderViewModel>? Organisations { get; set; }
    }   
}
