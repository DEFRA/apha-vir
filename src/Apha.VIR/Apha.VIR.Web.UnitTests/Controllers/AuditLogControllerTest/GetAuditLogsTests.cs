using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models.AuditLog;
using Apha.VIR.Web.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.AuditLogControllerTest
{
    public class GetAuditLogsTests
    {
        private readonly IAuditLogService _auditLogService;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly AuditLogController _controller;

        public GetAuditLogsTests()
        {
            _auditLogService = Substitute.For<IAuditLogService>();
            _cacheService = Substitute.For<ICacheService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new AuditLogController(_auditLogService, _cacheService, _mapper);
        }

        [Theory]
        [InlineData("submission")]
        [InlineData("sample")]
        [InlineData("isolate")]
        [InlineData("characteristics")]
        [InlineData("dispatch")]
        [InlineData("viability")]
        public async Task GetAuditLogs_ValidRequestType_ReturnsCorrectPartialView(string requestType)
        {
            // Arrange
            var expectedViewName = requestType == "viability" ? "_IsolateViabilityAuditLogResults"
       : string.Concat("_", requestType.AsSpan(0, 1).ToString().ToUpper(), requestType.AsSpan(1), "AuditLogResults");

            var auditLogService = Substitute.For<IAuditLogService>();
            var cacheService = Substitute.For<ICacheService>();
            var mapper = Substitute.For<IMapper>();
            var tempData = Substitute.For<ITempDataDictionary>();
            _cacheService.SetSessionValue("AuditLogSearchCriteria", "{\"AVNumber\":\"AV123\",\"DateTimeFrom\":\"2023-01-01\",\"DateTimeTo\":\"2023-12-31\",\"UserId\":\"testuser\"}");

            var controller = new AuditLogController(auditLogService, cacheService, mapper);

            // Act
            var result = await controller.GetAuditLogs(requestType);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            var partialViewResult = (PartialViewResult)result;

            Assert.Equal(expectedViewName, partialViewResult.ViewName);
        }

        [Fact]
        public async Task GetAuditLogs_InvalidRequestType_ReturnsSubmissionAuditLogs()
        {
            // Arrange
            var auditLogService = Substitute.For<IAuditLogService>();
            var cacheService = Substitute.For<ICacheService>();
            var mapper = Substitute.For<IMapper>();
            var tempData = Substitute.For<ITempDataDictionary>();
            _cacheService.SetSessionValue("AuditLogSearchCriteria", "{\"AVNumber\":\"AV123\",\"DateTimeFrom\":\"2023-01-01\",\"DateTimeTo\":\"2023-12-31\",\"UserId\":\"testuser\"}");

            var controller = new AuditLogController(auditLogService, cacheService, mapper);

            // Act
            var result = await controller.GetAuditLogs("invalid");

            // Assert
            Assert.IsType<PartialViewResult>(result);
            var partialViewResult = (PartialViewResult)result;
            Assert.Equal("_SubmissionAuditLogResults", partialViewResult.ViewName);
        }


        [Fact]
        public async Task GetAuditLogs_NullTempData_ReturnsEmptyPartialView()
        {
            // Arrange
            var auditLogService = Substitute.For<IAuditLogService>();
            var cacheService = Substitute.For<ICacheService>();
            var mapper = Substitute.For<IMapper>();
            var tempData = Substitute.For<ITempDataDictionary>();
            cacheService.SetSessionValue("AuditLogSearchCriteria", null!);

            var controller = new AuditLogController(auditLogService, cacheService, mapper);

            // Act
            var result = await controller.GetAuditLogs("submission");

            // Assert
            Assert.IsType<PartialViewResult>(result);
            var partialViewResult = (PartialViewResult)result;
            Assert.Equal("_SubmissionAuditLogResults", partialViewResult.ViewName);
            Assert.NotNull(partialViewResult.Model);
        }

        [Fact]
        public async Task GetAuditLogs_ValidTempData_CallsServiceAndMapsResult()
        {
            // Arrange
            var auditLogService = Substitute.For<IAuditLogService>();
            var cacheService = Substitute.For<ICacheService>();
            var mapper = Substitute.For<IMapper>();

            var searchCriteriaJson = "{\"AVNumber\":\"AV123\",\"DateTimeFrom\":\"2023-01-01\",\"DateTimeTo\":\"2023-12-31\",\"UserId\":\"testuser\"}";

            cacheService.GetSessionValue("AuditLogSearchCriteria").Returns(searchCriteriaJson!);

            auditLogService.GetSubmissionLogsAsync(Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<string>())
            .Returns(Task.FromResult<IEnumerable<AuditSubmissionLogDto>>(new[] { new AuditSubmissionLogDto() }));

            var controller = new AuditLogController(auditLogService, cacheService, mapper);

            // Act
            var result = await controller.GetAuditLogs("submission");

            // Assert
            Assert.IsType<PartialViewResult>(result);
            await auditLogService.Received(1).GetSubmissionLogsAsync(Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<string>());
            mapper.Received(1).Map<IEnumerable<AuditSubmissionLogModel>>(Arg.Any<object[]>());
        }
    }
}
