using System.ComponentModel.DataAnnotations;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Models
{

    public class IsolateRelocateViewModel
    {
        public Guid? IsolateId { get; set; }        
        public Guid? Freezer { get; set; }
        public Guid? Tray { get; set; }
        public string? Well { get; set; }
        public string? FreezerName { get; set; }
        public string? TrayName { get; set; }
        public string? AVNumber { get; set; }
        public string? Nomenclature { get; set; }
        public byte[]? LastModified { get; set; }
        public string UserID { get; set; } = string.Empty;
    }

    public class IsolateRelocationViewModel
    {
        public string? MinAVNumber { get; set; }
        public string? MaxAVNumber { get; set; }
        public Guid? SelectedFreezer { get; set; }
        public Guid? SelectedTray { get; set; }
        public List<IsolatedRelocationData>? SelectedNewIsolatedList { get; set; }
        public Guid? SelectedNewFreezer { get; set; }
        public Guid? SelectedNewTray { get; set; }
        public List<SelectListItem>? FreezersList { get; set; }
        public List<SelectListItem>? TraysList { get; set; }        
        public List<IsolateRelocateViewModel>? SearchResults { get; set; }
    }

    public class IsolatedRelocationData
    {
        public Guid? IsolatedId { get; set; }
        public string? Well { get; set; }
        public byte[]? LastModified { get; set; }
    }   

    public enum RelocationType  
    {
        Isolate,
        Tray
    }
}
