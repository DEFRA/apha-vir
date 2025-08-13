using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

public class EditSubmissionDetailsViewModel
{
    [Display(Name = "AV Number")]
    public string AVNumber { get; set; }

    [Display(Name = "RL Reference Number")]
    public string RLReferenceNumber { get; set; }

    [Display(Name = "Submitting Lab")]
    public string SubmittingLab { get; set; }
    public List<SelectListItem> SubmittingLabList { get; set; }

    [Display(Name = "Sender's Submission Reference Number")]
    [RegularExpression(@"[\da-zA-Z'.,\-\x5C\x20\/\(\)\#]*", ErrorMessage = "Letters and numbers only")]
    public string SendersReferenceNumber { get; set; }

    [Display(Name = "Submitting Country")]
    public string SubmittingCountry { get; set; }
    public List<SelectListItem> CountryList { get; set; }

    [Display(Name = "Sender")]
    [MaxLength(50)]
    public string Sender { get; set; }

    [Display(Name = "Sender Organisation")]
    [MaxLength(200)]
    public string SenderOrganisation { get; set; }

    [Display(Name = "Sender Address")]
    [MaxLength(500)]
    public string SenderAddress { get; set; }

    [Display(Name = "Country of Origin")]
    public string CountryOfOrigin { get; set; }

    [Display(Name = "Reason for Submission")]
    public string ReasonForSubmission { get; set; }
    public List<SelectListItem> ReasonList { get; set; }

    [Display(Name = "Date Submission Received")]
    [DataType(DataType.Date)]
    [Range(typeof(DateTime), "1900-01-01", "2100-12-31", ErrorMessage = "Date must be between 01/01/1900 and 31/12/2100")]
    public DateTime? DateSubmissionReceived { get; set; }

    [Display(Name = "No. Samples Received")]
    [Range(1, int.MaxValue, ErrorMessage = "Must be a positive number.")]
    public int? NumberOfSamples { get; set; }

    [Display(Name = "CPH Number")]
    [RegularExpression(@"\d{2}/\d{3}/\d{4}(/\d{2}|)", ErrorMessage = "Format is 00/000/0000 or 00/000/0000/00")]
    public string CPHNumber { get; set; }

    [Display(Name = "Owner")]
    public string Owner { get; set; }

    [Display(Name = "Sampling Location / Premises")]
    public string SamplingLocationPremises { get; set; }
}
