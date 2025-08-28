using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Apha.VIR.Web.Utilities
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
            {
                var errorMessage = FormatErrorMessage(validationContext.DisplayName);
                return new ValidationResult(errorMessage);
            }

            return ValidationResult.Success;
        }

        public override string FormatErrorMessage(string name)
        {
            if (!string.IsNullOrWhiteSpace(ErrorMessage) &&
                ErrorMessage.Contains("{0}") &&
                ErrorMessage.Contains("{1}"))
            {
                return string.Format(ErrorMessage, _minDate.ToString("dd/MM/yyyy"), DateTime.Today.ToString("dd/MM/yyyy"));
            }
            else
            {
                return ErrorMessage!;
            }
        }
    }
}
