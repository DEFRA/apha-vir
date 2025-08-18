using System.ComponentModel.DataAnnotations;

namespace Apha.VIR.Web.Models
{
    public class IsolateViabilityModel
    {
        [Required]
        public Guid IsolateViabilityId { get; set; }
        [Required]
        public Guid IsolateViabilityIsolateId { get; set; }
        [Required]
        public Guid Viable { get; set; }
        public string? ViabilityStatus { get; set; } = null!;
        [Required(ErrorMessage = "Please check this date.")]
        [DateRange("01/01/1900", ErrorMessage = "Please check this date.")]
        public DateTime DateChecked { get; set; }
        [Required]
        public Guid CheckedById { get; set; }
        [Required]
        public byte[] LastModified { get; set; } = null!;
        public string? CheckedByName { get; set; } = null!;
        public string? ViableName { get; set; } = null!;
        public string? Nomenclature { get; set; } = null!;
        public string? AVNumber { get; set; } = null!;
    }
}
