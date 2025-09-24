using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Apha.VIR.Web.Models.AuditLog;
using Apha.VIR.Application.DTOs;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Apha.VIR.Web.Services;

namespace Apha.VIR.Web.UnitTests.Controllers.AuditLogControllerTest
{
    public class SearchAuditTests
    {
        private readonly IAuditLogService _auditLogService;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly AuditLogController _controller;

        public SearchAuditTests()
        {
            _auditLogService = Substitute.For<IAuditLogService>();
            _cacheService = Substitute.For<ICacheService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new AuditLogController(_auditLogService, _cacheService, _mapper);
        }

        [Fact]
        public async Task SearchAudit_IsNewSearch_ValidModel_ReturnsViewWithViewModel()
        {
            // Arrange
            var searchCriteria = new AuditLogSearchModel
            {
                AVNumber = "AV000000-01",
                DateTimeFrom = DateTime.Now.AddDays(-7),
                DateTimeTo = DateTime.Now,
                UserId = "user123"
            };

            var submissionLogs = new List<AuditSubmissionLogDto> { new AuditSubmissionLogDto(), new AuditSubmissionLogDto() };

            _auditLogService.GetSubmissionLogsAsync(Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<string>())
            .Returns(submissionLogs);

            var mappedLogs = new List<AuditSubmissionLogModel> { new AuditSubmissionLogModel(), new AuditSubmissionLogModel() };
            _mapper.Map<IEnumerable<AuditSubmissionLogModel>>(Arg.Any<IEnumerable<object>>()).Returns(mappedLogs);

            _cacheService.SetSessionValue("AuditLogSearchCriteria", JsonConvert.SerializeObject(searchCriteria));   

            // Act
            ValidateModel(searchCriteria, _controller);
            var result = await _controller.SearchAudit(searchCriteria, true);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AuditLogViewModel>(viewResult.Model);
            Assert.Equal("AuditLog", viewResult.ViewName);
            Assert.Equal(searchCriteria.AVNumber, model.AVNumber);
            Assert.Equal(searchCriteria.DateTimeFrom, model.DateTimeFrom);
            Assert.Equal(searchCriteria.DateTimeTo, model.DateTimeTo);
            Assert.Equal(searchCriteria.UserId, model.UserId);
            Assert.False(model.ShowErrorSummary);
            Assert.True(model.IsNewSearch);
            Assert.Equal(mappedLogs, model.SubmissionLogs);
        }

        [Fact]
        public async Task SearchAudit_IsNewSearch_InvalidModel_ReturnsViewWithErrors()
        {
            // Arrange
            var searchCriteria = new AuditLogSearchModel();
            _controller.ModelState.AddModelError("AVNumber", "AVNumber is required");

            // Act
            var result = await _controller.SearchAudit(searchCriteria, true);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AuditLogViewModel>(viewResult.Model);
            Assert.Equal("AuditLog", viewResult.ViewName);
            Assert.False(model.ShowErrorSummary);
        }

        [Fact]
        public async Task SearchAudit_NotNewSearch_ValidTempData_ReturnsViewWithIsolateLogs()
        {
            // Arrange
            var searchCriteria = new AuditLogSearchModel
            {
                AVNumber = "AV000000-01",
                DateTimeFrom = DateTime.Now.AddDays(-7),
                DateTimeTo = DateTime.Now,
                UserId = "user123"
            };

            var searchCriteriaJson = System.Text.Json.JsonSerializer.Serialize(searchCriteria);

            _cacheService.SetSessionValue("AuditLogSearchCriteria", JsonConvert.SerializeObject(searchCriteria));
            _cacheService.GetSessionValue("AuditLogSearchCriteria").Returns(searchCriteriaJson!);

            var isolateLogs = new List<AuditIsolateLogDto> { new AuditIsolateLogDto(), new AuditIsolateLogDto() };
            _auditLogService.GetIsolatLogsAsync(Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<string>())
            .Returns(isolateLogs);

            var mappedLogs = new List<AuditIsolateLogModel> { new AuditIsolateLogModel(), new AuditIsolateLogModel() };
            _mapper.Map<IEnumerable<AuditIsolateLogModel>>(Arg.Any<IEnumerable<object>>()).Returns(mappedLogs);

            // Act
            var result = await _controller.SearchAudit(new AuditLogSearchModel(), false);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AuditLogViewModel>(viewResult.Model);
            Assert.Equal("AuditLog", viewResult.ViewName);
            Assert.Equal(searchCriteria.AVNumber, model.AVNumber);
            Assert.Equal(searchCriteria.DateTimeFrom, model.DateTimeFrom);
            Assert.Equal(searchCriteria.DateTimeTo, model.DateTimeTo);
            Assert.Equal(searchCriteria.UserId, model.UserId);
            Assert.False(model.ShowErrorSummary);
            Assert.False(model.IsNewSearch);
            Assert.Equal(mappedLogs, model.IsolateLogs);
        }

        [Fact]
        public async Task SearchAudit_NotNewSearch_InvalidTempData_ReturnsDefaultView()
        {
            // Arrange           
            _cacheService.SetSessionValue("AuditLogSearchCriteria", (string?)null);

            // Act
            var result = await _controller.SearchAudit(new AuditLogSearchModel(), false);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AuditLogViewModel>(viewResult.Model);
            Assert.Equal("AuditLog", viewResult.ViewName);
            Assert.False(model.ShowErrorSummary);
            Assert.True(model.ShowDefaultView);
        }

        private static void ValidateModel(object model, Controller controller)
        {
            var validationContext = new ValidationContext(model, null, null);
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(model, validationContext, validationResults, true);

            foreach (var validationResult in validationResults)
            {
                foreach (var memberName in validationResult.MemberNames)
                {
                    controller.ModelState.AddModelError(memberName, validationResult.ErrorMessage!);
                }
            }
        }

    }
}
