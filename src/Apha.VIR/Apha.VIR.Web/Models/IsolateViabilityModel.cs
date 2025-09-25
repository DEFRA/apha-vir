using System.ComponentModel.DataAnnotations;
using Apha.VIR.Web.Utilities;

namespace Apha.VIR.Web.Models
{
    public class IsolateViabilityModel
    {
        [Required]
        public required Guid IsolateViabilityId { get; set; }
        [Required]
        public required Guid IsolateViabilityIsolateId { get; set; }
        [Required]
        public required Guid Viable { get; set; }
        public string? ViabilityStatus { get; set; } = null!;
        [Required(ErrorMessage = "Please check this date.")]
        [DateRange("01/01/1900", ErrorMessage = "Please check this date.")]
        public required DateTime DateChecked { get; set; }
        [Required]
        public required Guid CheckedById { get; set; }
        [Required]
        public byte[] LastModified { get; set; } = null!;
        public string? CheckedByName { get; set; } = null!;
        public string? ViableName { get; set; } = null!;
        public string? Nomenclature { get; set; } = null!;
        public string? AVNumber { get; set; } = null!;
    }
}
