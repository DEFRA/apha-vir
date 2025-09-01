using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Models
{
    public class SenderMViewModel
    {
        public Guid? SenderId { get; set; }
        [Required(ErrorMessage = "Sender must be entered")]
        public required string SenderName { get; set; }
        [Required(ErrorMessage = "Sender Organisation must be entered")]
        public required string SenderOrganisation { get; set; }
        [Required(ErrorMessage = "Sender Address must be entered")]
        public required string SenderAddress { get; set; }
        public Guid? Country { get; set; }
        public string? CountryName { get; set; }
        public List<SelectListItem>? CountryList { get; set; }
        
    }
}
