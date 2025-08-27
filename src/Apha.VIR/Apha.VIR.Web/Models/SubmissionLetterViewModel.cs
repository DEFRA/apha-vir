using System.ComponentModel.DataAnnotations;

namespace Apha.VIR.Web.Models
{
    public class SubmissionLetterViewModel
    {
        [Display(Name = "Letter Content")]
        [DataType(DataType.MultilineText)]
        public string LetterContent { get; set; }

        public string BackUrl { get; set; }
    }
}
