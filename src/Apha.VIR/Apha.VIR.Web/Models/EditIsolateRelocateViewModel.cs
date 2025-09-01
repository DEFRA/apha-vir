using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Models
{
    public class EditIsolateRelocateViewModel
    {
        public Guid IsolateId { get; set; }

        [Display(Name = "AV Number")]
        public string AVNumber { get; set; }

        [Display(Name = "Nomenclature")]
        public string Nomenclature { get; set; }

        public Guid FreezerId { get; set; }

        public Guid TrayId { get; set; }

        [Display(Name = "Well")]
        public string Well { get; set; }

        public string FreezerName { get; set; }
        public string TrayName { get; set; }

        public List<SelectListItem> Freezers { get; set; }

        public List<SelectListItem> Trays { get; set; }
    }
}
