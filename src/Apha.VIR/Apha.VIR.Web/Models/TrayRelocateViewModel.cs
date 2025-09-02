using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Models
{
    public class TrayRelocateViewModel
    {
        public IEnumerable<SelectListItem> Freezers { get; set; }
        public IEnumerable<SelectListItem> Trays { get; set; }
        public IEnumerable<TrayRelocateIsolate> Isolates { get; set; }

        public Guid SelectedFreezerId { get; set; }
        public Guid SelectedTrayId { get; set; }
        public Guid TrayId { get; set; }
        public Guid NewFreezerId { get; set; }
    }


    public class TrayRelocateFreezer
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
    }

    public class TrayRelocateTray
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public Guid FreezerID { get; set; }
    }

    public class TrayRelocateIsolate
    {
        public Guid Id { get; set; }
        public string AVNumber { get; set; }
        public string Nomenclature { get; set; }
        public string FreezerName { get; set; }
        public string TrayName { get; set; }
        public string Well { get; set; }
    }
}
