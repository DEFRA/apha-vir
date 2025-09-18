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
    public class GetViabilityAuditLogsTests
    {
        private readonly IAuditLogService _auditLogService;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly AuditLogController _controller;

        public GetViabilityAuditLogsTests()
        {
            _auditLogService = Substitute.For<IAuditLogService>();
            _cacheService = Substitute.For<ICacheService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new AuditLogController(_auditLogService, _cacheService, _mapper);
        }

        [Fact]
        public async Task GetViabilityAuditLogs_WithCriteria_ReturnsPartialView()
        {
            var criteria = new AuditLogSearchModel { AVNumber = "AV123", UserId = "test" };           
            await _cacheService.SetCacheValueAsync("SearchCriteria", JsonConvert.SerializeObject(criteria));
           
            _auditLogService.GetIsolateViabilityLogsAsync(Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<string>())
                .Returns(new[] { new AuditViabilityLogDto() });

            _mapper.Map<IEnumerable<AuditIsolateViabilityLogModel>>(Arg.Any<object>())
                .Returns(new[] { new AuditIsolateViabilityLogModel() });

            var result = await _controller.GetAuditLogs("viability");

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_IsolateViabilityAuditLogResults", partial.ViewName);
            Assert.IsAssignableFrom<AuditIsolateViabilityLogModel>(partial.Model);
        }

        [Fact]
        public async Task GetViabilityAuditLogs_NoCriteria_ReturnsEmptyModel()
        {
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Substitute.For<ITempDataProvider>();
            var tempData = new TempDataDictionary(httpContext, tempDataProvider);
            tempData["SearchCriteria"] = null;
            _controller.TempData = tempData;

            var result = await _controller.GetAuditLogs("viability");

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_IsolateViabilityAuditLogResults", partial.ViewName);
            Assert.IsType<AuditIsolateViabilityLogModel>(partial.Model);
        }
    }
}
