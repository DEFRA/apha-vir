using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Models
{
    public class IsolateAddEditViewModel
    {
        public Guid? IsolateId { get; set; }
        public Guid? IsolateSampleId { get; set; }
        public int? IsolateNumber { get; set; }
        public string? AVNumber { get; set; }               
        public Guid? Family { get; set; }
        public string? FamilyName { get; set; }
        public Guid? Type { get; set; }

        [Display(Name = "Year Of Isolation")]        
        public int? YearOfIsolation { get; set; }
        public bool IsMixedIsolate { get; set; } = false;
        public Guid? IsolationMethod { get; set; }
        public bool AntiserumProduced { get; set; } = false;
        public bool AntigenProduced { get; set; } = false;
        public string? PhylogeneticAnalysis { get; set; }
        public string? MTALocation { get; set; }
        public string? Comment { get; set; }
        public bool ValidToIssue { get; set; } = true;
        public string? WhyNotValidToIssue { get; set; }
        public Guid? Freezer { get; set; }
        public Guid? Tray { get; set; }    
        public string? Well { get; set; }

        [Range(0, 4, ErrorMessage = "No of Aliquots must be between 0 and 4")]
        public int? NoOfAliquots { get; set; }
        public bool MaterialTransferAgreement { get; set; } = false;
        public bool OriginalSampleAvailable { get; set; } = false;

        [Display(Name = "First Viable Passage Number")]
        [RegularExpression(@"^\d+$", ErrorMessage = "First Viable Passage Number must be an numeric value")]
        public int? FirstViablePassageNumber { get; set; }
        public string? SmsreferenceNumber { get; set; }
        public string? PhylogeneticFileName { get; set; }
        public string? Nomenclature { get; set; }
        public string? IsolateNomenclature { get; set; }
        public Guid? Viable { get; set; }
        public DateTime? DateChecked { get; set; }
        public Guid? CheckedBy { get; set; }
        public string? ActionType { get; set; }
        public bool IsDetection { get; set; } = false;
        public bool IsViabilityInsert { get; set; } = false;
        public string? CreatedBy { get; set; }
        public Guid? PreviousViable { get; set; }
        public DateTime? PreviousDateChecked { get; set; }
        public Guid? PreviousCheckedBy { get; set; }
        public byte[] LastModified { get; set; } = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
        public List<SelectListItem>? VirusFamilyList { get; set; }
        public List<CustomSelectListItem>? VirusTypeList { get; set; }
        public List<SelectListItem>? IsolationMethodList { get; set; }
        public List<SelectListItem>? FreezerList { get; set; }
        public List<SelectListItem>? TrayList { get; set; }       
        public List<SelectListItem>? ViabilityList { get; set; }
        public List<SelectListItem>? StaffList { get; set; }
    }
}
