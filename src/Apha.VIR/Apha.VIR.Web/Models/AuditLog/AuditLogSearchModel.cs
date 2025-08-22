using System.ComponentModel.DataAnnotations;
using Apha.VIR.Web.Utilities;

namespace Apha.VIR.Web.Models.AuditLog
{
    public class AuditLogSearchModel
    {
        [Required(ErrorMessage = "AVNumber must be entered")]
        public string AVNumber { get; set; } = string.Empty;
        public DateTime? DateTimeFrom { get; set; }
        public DateTime? DateTimeTo { get; set; }
        public string? UserId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (DateTimeFrom != null && DateTimeFrom == DateTime.MinValue)
            {
                results.Add(new ValidationResult("Date/Time from is invalid"));
            }
            if (DateTimeTo != null && DateTimeTo == DateTime.MinValue)
            {
                results.Add(new ValidationResult("Date/Time to is invalid"));
            }

            if (DateTimeFrom != null && DateTimeTo != null && DateTimeTo < DateTimeFrom)
            {
                results.Add(new ValidationResult("Date/Time To must be after or equal to Date/Time From"));
            }

            if (string.IsNullOrWhiteSpace(AVNumber))
            {
                results.Add(new ValidationResult("AVNumber must be supplied"));
            }
            else if (!AVNumberUtil.AVNumberIsValidPotentially(AVNumber))
            {
                results.Add(new ValidationResult("AVNumber format must be AVnnnnnn-YY, PDnnnn-YY, SInnnnnn-YY or BNnnnnnn-YY"));
            }


            return results;
        }
    }
}
