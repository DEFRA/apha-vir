using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models.AuditLog;
using Apha.VIR.Web.Services;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.AuditLogControllerTest
{
    public class GetDispatchAuditLogsTests
    {
        private readonly IAuditLogService _auditLogService;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly AuditLogController _controller;

        public GetDispatchAuditLogsTests()
        {
            _auditLogService = Substitute.For<IAuditLogService>();
            _cacheService = Substitute.For<ICacheService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new AuditLogController(_auditLogService, _cacheService, _mapper);
        }

        [Fact]
        public async Task GetDispatchAuditLogs_WithCriteria_ReturnsPartialView()
        {
            var criteria = new AuditLogSearchModel { AVNumber = "AV123", UserId = "test" };
            _cacheService.SetSessionValue("AuditLogSearchCriteria", JsonConvert.SerializeObject(criteria));           

            _auditLogService.GetDispatchLogsAsync(Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<string>())
                .Returns(new[] { new AuditDispatchLogDto() });

            _mapper.Map<IEnumerable<AuditDispatchLogModel>>(Arg.Any<object>())
                .Returns(new[] { new AuditDispatchLogModel() });

            var result = await _controller.GetAuditLogs("dispatch");

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DispatchAuditLogResults", partial.ViewName);
            Assert.IsAssignableFrom<List<AuditDispatchLogModel>>(partial.Model);
        }

        [Fact]
        public async Task GetDispatchAuditLogs_NoCriteria_ReturnsEmptyModel()
        {
            var result = await _controller.GetAuditLogs("dispatch");

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DispatchAuditLogResults", partial.ViewName);
            Assert.IsType<List<AuditDispatchLogModel>>(partial.Model);
        }

        [Fact]
        public async Task GetDispatchAuditLogs_EmptyCriteriaString_ReturnsEmptyModel()
        {
            // Arrange
            _cacheService.SetSessionValue("AuditLogSearchCriteria", ""); // empty string

            // Act
            var result = await _controller.GetAuditLogs("dispatch");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DispatchAuditLogResults", partial.ViewName);
            Assert.IsType<List<AuditDispatchLogModel>>(partial.Model);
        }

        [Fact]
        public async Task GetDispatchAuditLogs_InvalidJsonCriteria_ReturnsEmptyModel()
        {
            // Arrange
            _cacheService.SetSessionValue("AuditLogSearchCriteria", "not a json");

            // Act
            var result = await _controller.GetAuditLogs("dispatch");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DispatchAuditLogResults", partial.ViewName);
            Assert.IsType<List<AuditDispatchLogModel>>(partial.Model);
        }

        [Fact]
        public async Task GetDispatchAuditLogs_DeserializesToNull_ReturnsEmptyModel()
        {
            // Arrange
            _cacheService.SetSessionValue("AuditLogSearchCriteria", "null");

            // Act
            var result = await _controller.GetAuditLogs("dispatch");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DispatchAuditLogResults", partial.ViewName);
            Assert.IsType<List<AuditDispatchLogModel>>(partial.Model);
        }
    }
}
