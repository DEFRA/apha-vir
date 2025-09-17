using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Web.Utilities;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Utilities
{
    public class DateRangeAttributeTests
    {
        [Fact]
        public void FormatErrorMessage_WithValidErrorMessageFormat_ReturnsFormattedMessage()
        {
            // Arrange
            var minDate = "01/01/2000";
            var attribute = new DateRangeAttribute(minDate)
            {
                ErrorMessage = "Date must be between {0} and {1}"
            };

            // Act
            var result = attribute.FormatErrorMessage("TestProperty");

            // Assert
            Assert.Contains("01/01/2000", result);
            Assert.Contains(DateTime.Today.ToString("dd/MM/yyyy"), result);
        }

        [Fact]
        public void FormatErrorMessage_WithInvalidErrorMessageFormat_ReturnsOriginalErrorMessage()
        {
            // Arrange
            var minDate = "01/01/2000";
            var attribute = new DateRangeAttribute(minDate)
            {
                ErrorMessage = "Invalid date"
            };

            // Act
            var result = attribute.FormatErrorMessage("TestProperty");

            // Assert
            Assert.Equal("Invalid date", result);
        }


        [Fact]
        public void Constructor_ValidMinDate_SetsMinDateCorrectly()
        {
            // Arrange
            string minDateString = "01/01/2000";

            // Act
            var attribute = new DateRangeAttribute(minDateString);

            // Assert
            Assert.Equal(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Unspecified), attribute.GetType().GetField("_minDate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(attribute));
        }

        [Fact]
        public void Constructor_InvalidMinDate_ThrowsFormatException()
        {
            // Arrange
            string invalidDateString = "invalid_date";

            // Act & Assert
            Assert.Throws<FormatException>(() => new DateRangeAttribute(invalidDateString));
        }

        [Fact]
        public void IsValid_NonDateTimeValue_ReturnsSuccess()
        {
            // Arrange
            var attribute = new DateRangeAttribute("01/01/2000");
            var context = new ValidationContext(new object());

            // Act
            var result = attribute.GetType().GetMethod("IsValid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(attribute, new object[] { "not a date", context });

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void IsValid_DateWithinRange_ReturnsSuccess()
        {
            // Arrange
            var attribute = new DateRangeAttribute("01/01/2000");
            var context = new ValidationContext(new object());
            var validDate = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Unspecified); // Added DateTimeKind

            // Act
            var result = attribute.GetType().GetMethod("IsValid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(attribute, new object[] { validDate, context });

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void IsValid_DateBeforeMinDate_ReturnsValidationResult()
        {
            // Arrange
            var attribute = new DateRangeAttribute("01/01/2000");
            var context = new ValidationContext(new object());
            var invalidDate = new DateTime(1999, 12, 31, 0, 0, 0, DateTimeKind.Unspecified); // Added DateTimeKind

            // Act
            var result = attribute.GetType().GetMethod("IsValid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(attribute, new object[] { invalidDate, context }) as ValidationResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(ValidationResult.Success, result);
        }

        [Fact]
        public void IsValid_DateAfterToday_ReturnsValidationResult()
        {
            // Arrange
            var attribute = new DateRangeAttribute("01/01/2000");
            var context = new ValidationContext(new object());
            var invalidDate = DateTime.Today.AddDays(1);

            // Act
            var result = attribute.GetType().GetMethod("IsValid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(attribute, new object[] { invalidDate, context }) as ValidationResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(ValidationResult.Success, result);
        }

    }
}
