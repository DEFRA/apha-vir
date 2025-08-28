using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Models

{
    public class IsolateRelocateViewModel
    {
        public string MinAVNumber { get; set; }
        public string MaxAVNumber { get; set; }
        public string SelectedFreezer { get; set; }
        public string SelectedTray { get; set; }

        public string SelectedNewFreezer { get; set; }
        public string SelectedNewTray { get; set; }
        public List<IsolateRelocateSelectListItem> Freezers { get; set; }
        public List<IsolateRelocateSelectListItem> Trays { get; set; }
        public List<IsolateRelocation> SearchResults { get; set; }

        public IEnumerable<SelectListItem> FreezerSelectList =>
            Freezers?.Select(f => new SelectListItem { Value = f.Value, Text = f.Text });

        public IEnumerable<SelectListItem> TraySelectList =>
            Trays?.Select(t => new SelectListItem { Value = t.Value, Text = t.Text });
    }

    public class IsolateRelocation
    {
        public Guid ID { get; set; }
        public string AVNumber { get; set; }
        public string Nomenclature { get; set; }
        public string FreezerName { get; set; }
        public string TrayName { get; set; }
        public string Well { get; set; }
        public bool IsSelected { get; set; }
    }

    public class IsolateRelocateSelectListItem
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }
}
