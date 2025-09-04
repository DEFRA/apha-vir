using System.ComponentModel.DataAnnotations;

namespace Apha.VIR.Web.Models
{
    public class SubmissionLetterViewModel
    {
        [DataType(DataType.MultilineText)]
        public string LetterContent { get; set; }

    }
}

