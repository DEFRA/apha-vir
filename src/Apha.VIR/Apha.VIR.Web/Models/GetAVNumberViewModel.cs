using System.ComponentModel.DataAnnotations;

namespace Apha.VIR.Web.Models
{
    public class GetAVNumberViewModel
    {
        [Required(ErrorMessage = "AV Number is required.")]
        [RegularExpression(@"^(AV\d{6}-\d{2}|PD\d{4}-\d{2}|SI\d{6}-\d{2}|BN\d{6}-\d{2})$",
    ErrorMessage = "Invalid AV Number format. Format should be AV000000-00, PD0000-00, SI000000-00 or BN000000-00.")]
        [Display(Name = "AV Number")]
        public string AVNumber { get; set; }

        public List<string> RecentAVNumbers { get; set; }

        public string ErrorMessage { get; set; }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public List<string> LastAVNumbers { get; set; }

        public GetAVNumberViewModel()
        {
            RecentAVNumbers = new List<string>();
        }


    }
}
