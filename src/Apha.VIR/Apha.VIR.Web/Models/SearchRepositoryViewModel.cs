using System.ComponentModel.DataAnnotations;
using Apha.VIR.Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Models
{
    public class SearchRepositoryViewModel
    {
        public string? AVNumber { get; set; }
        public Guid? VirusFamily { get; set; }
        public Guid? VirusType { get; set; }
        public Guid? Group { get; set; }
        public Guid? Species { get; set; }
        public Guid? CountryOfOrigin { get; set; }
        public Guid? HostPurpose { get; set; }
        public Guid? SampleType { get; set; }
        public int? YearOfIsolation { get; set; }
        public int? YearOfSubmissionRecieved { get; set; }
        public List<SelectListItem>? VirusFamilyList { get; set; }
        public List<SelectListItem>? VirusTypeList { get; set; }
        public List<SelectListItem>? VirusCharacteristicList { get; set; }
        public List<SelectListItem>? HostSpecyList { get; set; }
        public List<SelectListItem>? HostBreedList { get; set; }
        public List<SelectListItem>? CountryList { get; set; }
        public List<SelectListItem>? HostPurposeList { get; set; }
        public List<SelectListItem>? SampleTypeList { get; set; }
        public List<SelectListItem>? YearsList { get; set; }
        [Display(Name = "Created From Date")]
        public DateTime? CreatedFromDate { get; set; }
        [Display(Name = "Created To Date")]
        public DateTime? CreatedToDate { get; set; }
        [Display(Name = "Received From Date")]
        public DateTime? ReceivedFromDate { get; set; }
        [Display(Name = "Received To Date")]
        public DateTime? ReceivedToDate { get; set; }
        public List<CharacteristicSearchViewModel>? CharacteristicSearch { get; set; }
        public IsolateSearchGirdViewModel? IsolateSearchGird { get; set; }
        public bool IsFilterApplied { get; set; } = false;
    }
}
