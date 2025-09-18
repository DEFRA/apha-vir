using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models.AuditLog;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.AuditLogControllerTest
{
    public class GetIsolateAuditLogsTests
    {
        private readonly IAuditLogService _auditLogService;
        private readonly IMapper _mapper;
        private readonly AuditLogController _controller;

        public GetIsolateAuditLogsTests()
        {
            _auditLogService = Substitute.For<IAuditLogService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new AuditLogController(_auditLogService, _mapper);
        }

        [Fact]
        public async Task GetIsolateAuditLogs_WithCriteria_ReturnsPartialView()
        {
            var criteria = new AuditLogSearchModel { AVNumber = "AV123", UserId = "test" };
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Substitute.For<ITempDataProvider>();
            var tempData = new TempDataDictionary(httpContext, tempDataProvider);
            tempData["SearchCriteria"] = JsonConvert.SerializeObject(criteria);
            _controller.TempData = tempData;

            _auditLogService.GetIsolatLogsAsync(Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<string>())
                .Returns(new[] { new AuditIsolateLogDTO() });

            _mapper.Map<IEnumerable<AuditIsolateLogModel>>(Arg.Any<object>())
                .Returns(new[] { new AuditIsolateLogModel() });

            var result = await _controller.GetAuditLogs("isolate");

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_IsolateAuditLogResults", partial.ViewName);
            Assert.IsAssignableFrom<IEnumerable<AuditIsolateLogModel>>(partial.Model);
        }

        [Fact]
        public async Task GetIsolateAuditLogs_NoCriteria_ReturnsEmptyModel()
        {
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Substitute.For<ITempDataProvider>();
            var tempData = new TempDataDictionary(httpContext, tempDataProvider);
            tempData["SearchCriteria"] = null;
            _controller.TempData = tempData;

            var result = await _controller.GetAuditLogs("isolate");

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_IsolateAuditLogResults", partial.ViewName);
            Assert.IsType<AuditIsolateLogModel>(partial.Model);
        }
    }
}
