using System.ComponentModel.DataAnnotations;

namespace Apha.VIR.Web.Models
{
    public class GetAVNumberViewModel
    {
        [Required(ErrorMessage = "AV Number is required.")]       
        [Display(Name = "AV Number")]
        public string? AVNumber { get; set; }
        public List<string>? LastAVNumbers {  get; set; }
    }
}
