using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Apha.VIR.Web.Models
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class DateRangeAttribute : ValidationAttribute
    {
        private readonly DateTime _minDate;

        public DateRangeAttribute(string minDate)
        {
            _minDate = DateTime.ParseExact(minDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not DateTime dateValue)
                return ValidationResult.Success;

            if (dateValue < _minDate || dateValue > DateTime.Today)
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }
    }
}
