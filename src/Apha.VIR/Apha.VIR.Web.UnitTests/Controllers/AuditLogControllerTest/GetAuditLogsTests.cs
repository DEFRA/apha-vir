using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Apha.VIR.Web.Models.AuditLog;
using Apha.VIR.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Apha.VIR.Web.UnitTests.Controllers.AuditLogControllerTest
{
    public class GetAuditLogsTests
    {
        private readonly IAuditLogService _auditLogService;
        private readonly IMapper _mapper;
        private readonly AuditLogController _controller;

        public GetAuditLogsTests()
        {
            _auditLogService = Substitute.For<IAuditLogService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new AuditLogController(_auditLogService, _mapper);
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
                : $"_{requestType.Substring(0, 1).ToUpper() + requestType.Substring(1)}AuditLogResults";

            var auditLogService = Substitute.For<IAuditLogService>();
            var mapper = Substitute.For<IMapper>();
            var tempData = Substitute.For<ITempDataDictionary>();
            tempData["SearchCriteria"] = "{\"AVNumber\":\"AV123\",\"DateTimeFrom\":\"2023-01-01\",\"DateTimeTo\":\"2023-12-31\",\"UserId\":\"testuser\"}";

            var controller = new AuditLogController(auditLogService, mapper)
            {
                TempData = tempData
            };

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
            var mapper = Substitute.For<IMapper>();
            var tempData = Substitute.For<ITempDataDictionary>();
            tempData["SearchCriteria"] = "{\"AVNumber\":\"AV123\",\"DateTimeFrom\":\"2023-01-01\",\"DateTimeTo\":\"2023-12-31\",\"UserId\":\"testuser\"}";

            var controller = new AuditLogController(auditLogService, mapper)
            {
                TempData = tempData
            };

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
            var mapper = Substitute.For<IMapper>();
            var tempData = Substitute.For<ITempDataDictionary>();
            tempData["SearchCriteria"] = null;

            var controller = new AuditLogController(auditLogService, mapper)
            {
                TempData = tempData
            };

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
            var mapper = Substitute.For<IMapper>();
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Substitute.For<ITempDataProvider>();
            var tempData = new TempDataDictionary(httpContext, tempDataProvider);
            tempData["SearchCriteria"] = "{\"AVNumber\":\"AV123\",\"DateTimeFrom\":\"2023-01-01\",\"DateTimeTo\":\"2023-12-31\",\"UserId\":\"testuser\"}";

            auditLogService.GetSubmissionLogsAsync(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<string>())
            .Returns(new AuditSubmissionLogDTO[] { new AuditSubmissionLogDTO() });


           
            _controller.TempData = tempData;

            var controller = new AuditLogController(auditLogService, mapper)
            {
                TempData = tempData
            };

            // Act
            var result = await controller.GetAuditLogs("submission");

            // Assert
            Assert.IsType<PartialViewResult>(result);
            await auditLogService.Received(1).GetSubmissionLogsAsync(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<string>());
            mapper.Received(1).Map<IEnumerable<AuditSubmissionLogModel>>(Arg.Any<object[]>());
        }
    }
}
