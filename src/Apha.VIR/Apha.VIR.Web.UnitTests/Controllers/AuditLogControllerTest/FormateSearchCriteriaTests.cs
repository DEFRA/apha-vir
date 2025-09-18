using System.Reflection;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models.AuditLog;

namespace Apha.VIR.Web.UnitTests.Controllers.AuditLogControllerTest
{
    public class FormateSearchCriteriaTests
    {
        private static void InvokeFormateSearchCriteria(AuditLogSearchModel model)
        {
            var method = typeof(AuditLogController)
                .GetMethod("FormateSearchCriteria", BindingFlags.NonPublic | BindingFlags.Static);

            method!.Invoke(null, new object[] { model });
        }

        [Fact]
        public void FormateSearchCriteria_SetsDefaultDatesAndFormatsUserId()
        {
            // Arrange
            var model = new AuditLogSearchModel
            {
                AVNumber = "av123",
                DateTimeFrom = null,
                DateTimeTo = null,
                UserId = null
            };

            // Act
            InvokeFormateSearchCriteria(model);

            // Assert
            Assert.NotNull(model.DateTimeFrom);
            Assert.NotNull(model.DateTimeTo);
            Assert.Equal("%", model.UserId);
            Assert.StartsWith("AV", model.AVNumber); // AVNumberUtil formats with uppercase prefix
        }

        [Fact]
        public void FormateSearchCriteria_AppendsWildcardToUserId()
        {
            var model = new AuditLogSearchModel { AVNumber = "AV123", UserId = "abc" };

            InvokeFormateSearchCriteria(model);

            Assert.Equal("%abc%", model.UserId);
        }

        [Fact]
        public void FormateSearchCriteria_AdjustsDateTimeToIfMidnight()
        {
            var model = new AuditLogSearchModel
            {
                AVNumber = "AV123",
                DateTimeTo = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Unspecified) 
            };

            InvokeFormateSearchCriteria(model);

            Assert.Equal(0, model.DateTimeTo?.Minute);
            Assert.Equal(0, model.DateTimeTo?.Second);
            Assert.Equal(0, model.DateTimeTo?.Millisecond);
            Assert.Equal(0, model.DateTimeTo?.Hour); 
        }
    }
}
