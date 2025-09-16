using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models.AuditLog;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Web.UnitTests.Controllers.AuditLogControllerTest
{
    public class GetIsolateLogDetailTests
    {
        private readonly IAuditLogService _auditLogService;
        private readonly IMapper _mapper;
        private readonly AuditLogController _controller;

        public GetIsolateLogDetailTests()
        {
            _auditLogService = Substitute.For<IAuditLogService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new AuditLogController(_auditLogService, _mapper);
        }


        [Fact]
        public async Task GetIsolateLogDetail_InvalidModelState_ReturnsViewWithEmptyModel()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.GetIsolateLogDetail(Guid.NewGuid(), "AV123");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("IsolateAuditLogDetail", viewResult.ViewName);
            var model = Assert.IsType<AuditIsolateLogDetailsViewModel>(viewResult.Model);
            Assert.Equal("AV123", model.AVNumber);
        }


        [Fact]
        public async Task GetIsolateLogDetail_ValidModelState_ReturnsViewWithMappedModel()
        {
            // Arrange
            var logId = Guid.NewGuid();
            var avNumber = "AV123";
            var serviceResult = new AuditIsolateLogDetailDto();
            var mappedResult = new AuditIsolateLogDetailsViewModel();

            _auditLogService.GetIsolatLogDetailAsync(logId).Returns(serviceResult);
            _mapper.Map<AuditIsolateLogDetailsViewModel>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetIsolateLogDetail(logId, avNumber);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("IsolateAuditLogDetail", viewResult.ViewName);
            var model = Assert.IsType<AuditIsolateLogDetailsViewModel>(viewResult.Model);
            Assert.Equal(avNumber, model.AVNumber);
            await _auditLogService.Received(1).GetIsolatLogDetailAsync(logId);
            _mapper.Received(1).Map<AuditIsolateLogDetailsViewModel>(serviceResult);
        }

        [Fact]
        public async Task GetIsolateLogDetail_ServiceThrowsException_ThrowsException()
        {
            // Arrange
            var logId = Guid.NewGuid();
            var avNumber = "AV123";
            _auditLogService.GetIsolatLogDetailAsync(logId).Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetIsolateLogDetail(logId, avNumber));
        }
    }
}
