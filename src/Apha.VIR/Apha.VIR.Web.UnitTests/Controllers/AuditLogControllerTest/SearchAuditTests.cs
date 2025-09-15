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

namespace Apha.VIR.Web.UnitTests.Controllers.AuditLogControllerTest
{
    public class SearchAuditTests
    {
        private readonly IAuditLogService _auditLogService;
        private readonly IMapper _mapper;
        private readonly AuditLogController _controller;

        public SearchAuditTests()
        {
            _auditLogService = Substitute.For<IAuditLogService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new AuditLogController(_auditLogService, _mapper);
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

            var submissionLogs = new List<AuditSubmissionLogDTO> { new AuditSubmissionLogDTO(), new AuditSubmissionLogDTO() };

            _auditLogService.GetSubmissionLogsAsync(Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<string>())
            .Returns(submissionLogs);

            var mappedLogs = new List<AuditSubmissionLogModel> { new AuditSubmissionLogModel(), new AuditSubmissionLogModel() };
            _mapper.Map<IEnumerable<AuditSubmissionLogModel>>(Arg.Any<IEnumerable<object>>()).Returns(mappedLogs);

            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Substitute.For<ITempDataProvider>();
            var tempData = new TempDataDictionary(httpContext, tempDataProvider);

            tempData["SearchCriteria"] = JsonConvert.SerializeObject(searchCriteria);

            _controller.TempData = tempData;

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

            // Use real TempDataDictionary
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Substitute.For<ITempDataProvider>();
            var tempData = new TempDataDictionary(httpContext, tempDataProvider);

            tempData["SearchCriteria"] = JsonConvert.SerializeObject(searchCriteria);

            _controller.TempData = tempData;

            var isolateLogs = new List<AuditIsolateLogDTO> { new AuditIsolateLogDTO(), new AuditIsolateLogDTO() };
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
            // Use real TempDataDictionary
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = Substitute.For<ITempDataProvider>();
            var tempData = new TempDataDictionary(httpContext, tempDataProvider);
            tempData["SearchCriteria"] = null;
            _controller.TempData = tempData;

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
