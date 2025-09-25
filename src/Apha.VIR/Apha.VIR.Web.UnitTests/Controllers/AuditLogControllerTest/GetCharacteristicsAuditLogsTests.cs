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
    public class GetCharacteristicsAuditLogsTests
    {
        private readonly IAuditLogService _auditLogService;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly AuditLogController _controller;

        public GetCharacteristicsAuditLogsTests()
        {
            _auditLogService = Substitute.For<IAuditLogService>();
            _cacheService = Substitute.For<ICacheService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new AuditLogController(_auditLogService, _cacheService, _mapper);
        }

        [Fact]
        public async Task GetCharacteristicsAuditLogs_WithCriteria_ReturnsPartialView()
        {
            var criteria = new AuditLogSearchModel { AVNumber = "AV123", UserId = "test" };          
            _cacheService.SetSessionValue("AuditLogSearchCriteria", JsonConvert.SerializeObject(criteria));
           

            _auditLogService.GetCharacteristicsLogsAsync(Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<string>())
                .Returns(new[] { new AuditCharacteristicLogDto() });

            _mapper.Map<IEnumerable<AuditCharacteristicsLogModel>>(Arg.Any<object>())
                .Returns(new[] { new AuditCharacteristicsLogModel() });

            var result = await _controller.GetAuditLogs("characteristics");

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CharacteristicsAuditLogResults", partial.ViewName);
            Assert.IsAssignableFrom<List<AuditCharacteristicsLogModel>>(partial.Model);
        }

        [Fact]
        public async Task GetCharacteristicsAuditLogs_NoCriteria_ReturnsEmptyModel()
        {   
            var result = await _controller.GetAuditLogs("characteristics");

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CharacteristicsAuditLogResults", partial.ViewName);
            Assert.IsType<List<AuditCharacteristicsLogModel>>(partial.Model);
        }

        [Fact]
        public async Task GetCharacteristicsAuditLogs_EmptyCriteriaString_ReturnsEmptyModel()
        {
            _cacheService.SetSessionValue("AuditLogSearchCriteria", "");
            var result = await _controller.GetAuditLogs("characteristics");
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CharacteristicsAuditLogResults", partial.ViewName);
            Assert.IsType<List<AuditCharacteristicsLogModel>>(partial.Model);
        }

        [Fact]
        public async Task GetCharacteristicsAuditLogs_InvalidJsonCriteria_ReturnsEmptyModel()
        {
            _cacheService.SetSessionValue("AuditLogSearchCriteria", "not a json");
            var result = await _controller.GetAuditLogs("characteristics");
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CharacteristicsAuditLogResults", partial.ViewName);
            Assert.IsType<List<AuditCharacteristicsLogModel>>(partial.Model);
        }

        [Fact]
        public async Task GetCharacteristicsAuditLogs_DeserializesToNull_ReturnsEmptyModel()
        {
            _cacheService.SetSessionValue("AuditLogSearchCriteria", "null");
            var result = await _controller.GetAuditLogs("characteristics");
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CharacteristicsAuditLogResults", partial.ViewName);
            Assert.IsType<List<AuditCharacteristicsLogModel>>(partial.Model);
        }
    }
}
