using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.ReportsControllerTest
{
    public class ReportsControllerExportToExcelTests
    {
        private readonly IReportService _mockReportService;
        private readonly IMapper _mockMapper;
        private readonly ReportsController _controller;

        public ReportsControllerExportToExcelTests()
        {
            _mockReportService = Substitute.For<IReportService>();
            _mockMapper = Substitute.For<IMapper>();
            _controller = new ReportsController(_mockReportService, _mockMapper);
        }

        [Fact]
        public async Task ExportToExcel_ValidInputWithData_ReturnsFileResultWithCorrectContentTypeAndFileName()
        {
            // Arrange
            _controller.ModelState.Clear();

            var dateFrom = new DateTime(2023, 5, 1);
            var dateTo = new DateTime(2023, 5, 10);

            var inputModel = new IsolateDispatchReportViewModel
            {
                DateFrom = dateFrom,
                DateTo = dateTo
            };

            var serviceData = new List<IsolateDispatchReportDTO>
                                    {
                                        new IsolateDispatchReportDTO
                                        {
                                            AVNumber = "AV001",
                                            Nomenclature = "Virus X",
                                            NoOfAliquots = 10,
                                            PassageNumber = 2,
                                            Recipient = "Lab A",
                                            RecipientName = "Dr. Smith",
                                            RecipientAddress = "123 Research Way",
                                            ReasonForDispatch = "Testing",
                                            DispatchedDate = new DateTime(2023, 5, 5),
                                            DispatchedByName = "Technician A"
                                        }
                                    };

            var mappedData = serviceData.Select(dto => new IsolateDispatchReportModel
            {
                AVNumber = dto.AVNumber!,
                Nomenclature = dto.Nomenclature!,
                NoOfAliquots = dto.NoOfAliquots,
                PassageNumber = dto.PassageNumber,
                Recipient = dto.Recipient,
                RecipientName = dto.RecipientName,
                RecipientAddress = dto.RecipientAddress,
                ReasonForDispatch = dto.ReasonForDispatch,
                DispatchedDate = dto.DispatchedDate,
                DispatchedByName = dto.DispatchedByName
            }).ToList();

            _mockReportService.GetDispatchesReportAsync(dateFrom, dateTo).Returns(serviceData);
            _mockMapper.Map<IEnumerable<IsolateDispatchReportModel>>(serviceData).Returns(mappedData);

            // Act
            var result = await _controller.ExportToExcel(inputModel);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.NotNull(fileResult.FileContents);
            Assert.NotEmpty(fileResult.FileContents);

            // Optional: Validate file name
            var expectedFileNamePart = "VIR IsolateDispatchReport";
            Assert.Contains(expectedFileNamePart, fileResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportToExcel_ValidInputWithNoData_ReturnsFileResultWithEmptyWorksheet()
        {
            // Arrange
            var today = DateTime.Today;
            var expectedFileName = $"VIR IsolateDispatchReport {today.Day}{today:MMMM}{today.Year}.xlsx";

            _controller.ModelState.Clear();

            var model = new IsolateDispatchReportViewModel
            {
                DateFrom = new DateTime(2023, 1, 1),
                DateTo = new DateTime(2023, 1, 31)
            };

            _mockReportService.GetDispatchesReportAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(Task.FromResult<IEnumerable<IsolateDispatchReportDTO>>(new List<IsolateDispatchReportDTO>()));

            _mockMapper.Map<IEnumerable<IsolateDispatchReportModel>>(Arg.Any<IEnumerable<IsolateDispatchReportDTO>>())
            .Returns(new List<IsolateDispatchReportModel>());

            // Act
            var result = await _controller.ExportToExcel(model);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.Equal(expectedFileName, fileResult.FileDownloadName);
            Assert.True(fileResult.FileContents.Length > 0);
        }

        [Fact]
        public async Task ExportToExcel_InvalidModelState_ReturnsViewResult()
        {
            // Arrange
            var model = new IsolateDispatchReportViewModel();
            _controller.ModelState.AddModelError("DateFrom", "Required");

            // Act
            var result = await _controller.ExportToExcel(model);

            // Assert
            Assert.IsType<ViewResult>(result);
        }
    }
}
