using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Models
{
    public class IsolateViabilityViewModel
    {
        public List<SelectListItem>? ViabilityList { get; set; }
        public List<SelectListItem>? CheckedByList { get; set; }

        public required IsolateViabilityModel IsolateViability { get; set; }
    }
}
