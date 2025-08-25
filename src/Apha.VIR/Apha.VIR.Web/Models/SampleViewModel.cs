using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.VIR.Web.Models
{
    public class SampleViewModel
    {
        public Guid SampleId { get; set; }
        public Guid SampleSubmissionID { get; set; }        
        public string? AVNumber { get; set; }       
        public string? SampleNumber { get; set; }        
        public string? SMSReferenceNumber { get; set; }

        [RegularExpression(@"[\da-zA-Z'.,\x2D\x5C\x20\x2F\x28\x29\x23]*", ErrorMessage = "Letters, numbers -#/\\() allowed")]
        public string? SenderReferenceNumber { get; set; }
        public Guid? SampleType { get; set; }
        public Guid? HostSpecies { get; set; }
        public Guid? HostBreed { get; set; }
        public Guid? HostPurpose { get; set; }
        public string? SamplingLocationHouse { get; set; }
        public byte[]? LastModified { get; set; }
        public List<SelectListItem>? SampleTypeList { get; set; }
        public List<SelectListItem>? HostSpeciesList { get; set; }
        public List<SelectListItem>? HostBreedList { get; set; }
        public List<SelectListItem>? HostPurposeList { get; set; }
        public List<LatinBreed>? LatinBreedList { get; set; }
        public bool IsEditMode { get; set; }        
    }

    public class LatinBreed
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string ParentName { get; set; } = null!;
        public string AlternateName { get; set; } = null!;
        public bool Active { get; set; }
        public bool Sms { get; set; }
        public string? Smscode { get; set; }
    }
}
