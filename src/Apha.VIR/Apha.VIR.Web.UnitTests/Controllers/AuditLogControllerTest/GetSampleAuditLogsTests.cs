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
    public class GetSampleAuditLogsTests
    {
        private readonly IAuditLogService _auditLogService;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly AuditLogController _controller;

        public GetSampleAuditLogsTests()
        {
            _auditLogService = Substitute.For<IAuditLogService>();
            _cacheService = Substitute.For<ICacheService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new AuditLogController(_auditLogService, _cacheService, _mapper);
        }

        [Fact]
        public async Task GetSampleAuditLogs_WithCriteria_ReturnsPartialView()
        {
            var criteria = new AuditLogSearchModel { AVNumber = "AV123", UserId = "test" };           
            await _cacheService.SetCacheValueAsync("SearchCriteria", JsonConvert.SerializeObject(criteria));
            

            _auditLogService.GetSamplLogsAsync(Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<string>())
                .Returns(new[] { new AuditSampleLogDto() });

            _mapper.Map<IEnumerable<AuditSampleLogModel>>(Arg.Any<object>())
                .Returns(new[] { new AuditSampleLogModel() });

            var result = await _controller.GetAuditLogs("sample");

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_SampleAuditLogResults", partial.ViewName);
            Assert.IsAssignableFrom<AuditSampleLogModel>(partial.Model);
        }

        [Fact]
        public async Task GetSampleAuditLogs_NoCriteria_ReturnsEmptyModel()
        {
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Substitute.For<ITempDataProvider>();
            var tempData = new TempDataDictionary(httpContext, tempDataProvider);
            tempData["SearchCriteria"] = null;
            _controller.TempData = tempData;

            var result = await _controller.GetAuditLogs("sample");

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_SampleAuditLogResults", partial.ViewName);
            Assert.IsType<AuditSampleLogModel>(partial.Model);
        }

        [Fact]
        public async Task GetSampleAuditLogs_EmptyCriteriaString_ReturnsEmptyModel()
        {
            // Arrange
            await _cacheService.SetCacheValueAsync("SearchCriteria", ""); // empty string

            // Act
            var result = await _controller.GetAuditLogs("sample");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_SampleAuditLogResults", partial.ViewName);
            Assert.IsType<AuditSampleLogModel>(partial.Model);
        }

        [Fact]
        public async Task GetSampleAuditLogs_InvalidJsonCriteria_ReturnsEmptyModel()
        {
            // Arrange
            await _cacheService.SetCacheValueAsync("SearchCriteria", "not a json");

            // Act
            var result = await _controller.GetAuditLogs("sample");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_SampleAuditLogResults", partial.ViewName);
            Assert.IsType<AuditSampleLogModel>(partial.Model);
        }

        [Fact]
        public async Task GetSampleAuditLogs_DeserializesToNull_ReturnsEmptyModel()
        {
            // Arrange
            // This will deserialize to null if AuditLogSearchModel is a class and not a struct
            await _cacheService.SetCacheValueAsync("SearchCriteria", "null");

            // Act
            var result = await _controller.GetAuditLogs("sample");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_SampleAuditLogResults", partial.ViewName);
            Assert.IsType<AuditSampleLogModel>(partial.Model);
        }

    }
}
