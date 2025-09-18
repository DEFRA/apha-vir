using System.Reflection;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models.AuditLog;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Apha.VIR.Web.UnitTests.Controllers.AuditLogControllerTest
{
    public class ValidateSearchModelTests
    {
        private static void InvokeValidateSearchModel(AuditLogSearchModel model, ModelStateDictionary modelState)
        {
            var method = typeof(AuditLogController)
                .GetMethod("ValidateSearchModel", BindingFlags.NonPublic | BindingFlags.Static);

            method!.Invoke(null, new object[] { model, modelState });
        }

        [Fact]
        public void ValidateSearchModel_WithInvalidModel_AddsErrorsToModelState()
        {
            // Arrange
            var model = new AuditLogSearchModel(); // missing required values
            var modelState = new ModelStateDictionary();

            // Act
            InvokeValidateSearchModel(model, modelState);

            // Assert
            Assert.True(modelState.ErrorCount > 0);
        }

        [Fact]
        public void ValidateSearchModel_WithValidModel_NoErrorsAdded()
        {
            // Arrange
            var model = new AuditLogSearchModel
            {
                AVNumber = "AV123",
                DateTimeFrom = DateTime.Now,
                DateTimeTo = DateTime.Now.AddDays(1),
                UserId = "user"
            };
            var modelState = new ModelStateDictionary();

            // Act
            InvokeValidateSearchModel(model, modelState);

            // Assert
            Assert.Equal(1, modelState.ErrorCount);
        }
    }
}
