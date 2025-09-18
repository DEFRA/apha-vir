using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models.AuditLog;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.AuditLogControllerTest
{
    public class IndexTests
    {
        [Fact]
        public void Index_ReturnsAuditLogViewWithDefaultModel()
        {
            // Arrange
            var auditLogService = Substitute.For<IAuditLogService>();
            var mapper = Substitute.For<IMapper>();
            var controller = new AuditLogController(auditLogService, mapper);

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("AuditLog", viewResult.ViewName);
            var model = Assert.IsType<AuditLogViewModel>(viewResult.Model);
            Assert.False(model.ShowErrorSummary);
            Assert.True(model.ShowDefaultView);
        }
    }
}
