using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.ReportsControllerTest
{
    public class GenerateReportTests
    {
        private readonly IReportService _mockReportService;
        private readonly IMapper _mockMapper;
        private readonly ReportsController _controller;

        public GenerateReportTests()
        {
            _mockReportService = Substitute.For<IReportService>();
            _mockMapper = Substitute.For<IMapper>();
            _controller = new ReportsController(_mockReportService, _mockMapper);
        }
        [Fact]
        public async Task GenerateReport_ValidModelWithData_ReturnsViewWithPopulatedViewModel()
        {
            // Arrange
            var serviceResult = new List<IsolateDispatchReportDTO>
            {   new IsolateDispatchReportDTO{ AVNumber = "AV001" },
                new IsolateDispatchReportDTO{ AVNumber = "AV001" }
            };
            var mappedResult = new List<IsolateDispatchReportModel>
            {
                new IsolateDispatchReportModel { AVNumber = "AV001" },
                new IsolateDispatchReportModel { AVNumber = "AV002" }
            };
            var model = new IsolateDispatchReportViewModel
            {
                DateFrom = DateTime.Today.AddDays(-7),
                DateTo = DateTime.Today,
                ReportData = mappedResult
            };

            _mockReportService.GetDispatchesReportAsync(model.DateFrom, model.DateTo).Returns(serviceResult);
            _mockMapper.Map<IEnumerable<IsolateDispatchReportModel>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GenerateReport(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("IsolateDispatchReport", result.ViewName);
            var viewModel = Assert.IsType<IsolateDispatchReportViewModel>(result.Model);
            Assert.Equal(model.DateFrom, viewModel.DateFrom);
            Assert.Equal(model.DateTo, viewModel.DateTo);
            Assert.Equal(mappedResult, viewModel.ReportData);
        }

        [Fact]
        public async Task GenerateReport_InvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var model = new IsolateDispatchReportViewModel();
            _controller.ModelState.AddModelError("DateFrom", "Required");

            // Act
            var result = await _controller.GenerateReport(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(model, result.Model);
            await _mockReportService.DidNotReceive().GetDispatchesReportAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>());
        }

        [Fact]
        public async Task GenerateReport_EmptyReportData_ReturnsViewWithEmptyReportData()
        {
            // Arrange
            var model = new IsolateDispatchReportViewModel
            {
                DateFrom = DateTime.Today.AddDays(-7),
                DateTo = DateTime.Today
            };
            var serviceResult = new List<IsolateDispatchReportDTO>();
            var mappedResult = new List<IsolateDispatchReportModel>();

            _mockReportService.GetDispatchesReportAsync(model.DateFrom, model.DateTo).Returns(serviceResult);
            _mockMapper.Map<IEnumerable<IsolateDispatchReportModel>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GenerateReport(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("IsolateDispatchReport", result.ViewName);
            var viewModel = Assert.IsType<IsolateDispatchReportViewModel>(result.Model);
            Assert.Equal(model.DateFrom, viewModel.DateFrom);
            Assert.Equal(model.DateTo, viewModel.DateTo);
            Assert.Empty(viewModel.ReportData);
        }
    }
}
