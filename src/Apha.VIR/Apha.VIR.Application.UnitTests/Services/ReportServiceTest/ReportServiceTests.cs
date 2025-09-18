using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;

namespace Apha.VIR.Application.UnitTests.Services.ReportServiceTest
{
    public class ReportServiceTests
    {
        private readonly IReportRepository _mockReportRepository;
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly ReportService _reportService;

        public ReportServiceTests()
        {
            _mockReportRepository = Substitute.For<IReportRepository>();
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _reportService = new ReportService(_mockReportRepository, _mockLookupRepository, _mockMapper);
        }

        [Fact]
        public async Task GetDispatchesReportAsync_WithValidDateRange_ReturnsExpectedResult()
        {
            // Arrange
            var dateFrom = new DateTime(2023, 1, 1);
            var dateTo = new DateTime(2023, 12, 31);
            var repoResult = new List<IsolateDispatchInfo> { new IsolateDispatchInfo() };
            var expectedResult = new List<IsolateDispatchReportDto> { new IsolateDispatchReportDto() };

            _mockReportRepository.GetDispatchesReportAsync(dateFrom, dateTo).Returns(repoResult);
            _mockMapper.Map<IEnumerable<IsolateDispatchReportDto>>(repoResult).Returns(expectedResult);

            // Act
            var result = await _reportService.GetDispatchesReportAsync(dateFrom, dateTo);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetDispatchesReportAsync_WithNullDateRange_CallsRepositoryWithNullDates()
        {
            // Arrange
            _mockReportRepository.GetDispatchesReportAsync(null, null).Returns(new List<IsolateDispatchInfo>());

            // Act
            await _reportService.GetDispatchesReportAsync(null, null);

            // Assert
            await _mockReportRepository.Received(1).GetDispatchesReportAsync(null, null);
        }

        [Fact]
        public async Task GetDispatchesReportAsync_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            _mockReportRepository.GetDispatchesReportAsync(Arg.Any<DateTime?>(), Arg.Any<DateTime?>())
            .Returns(new List<IsolateDispatchInfo>());
            _mockMapper.Map<IEnumerable<IsolateDispatchReportDto>>(Arg.Any<IEnumerable<IsolateDispatchReport>>())
            .Returns(new List<IsolateDispatchReportDto>());

            // Act
            var result = await _reportService.GetDispatchesReportAsync(null, null);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDispatchesReportAsync_MapsRecipientAndDispatchedByNames()
        {
            var staffId = Guid.NewGuid();
            var dispatchId = Guid.NewGuid();

            // Arrange
            var repoResult = new List<IsolateDispatchInfo>
            {
            new IsolateDispatchInfo { RecipientId = staffId, DispatchedById = dispatchId }
            };
            var workgroups = new List<LookupItem> { new LookupItem { Id = staffId, Name = "Workgroup1" } };
            var staffs = new List<LookupItem> { new LookupItem { Id = dispatchId, Name = "Staff1" } };

            _mockReportRepository.GetDispatchesReportAsync(Arg.Any<DateTime?>(), Arg.Any<DateTime?>()).Returns(repoResult);
            _mockLookupRepository.GetAllWorkGroupsAsync().Returns(workgroups);
            _mockLookupRepository.GetAllStaffAsync().Returns(staffs);

            // Act
            await _reportService.GetDispatchesReportAsync(null, null);

            // Assert
            Assert.Equal("Workgroup1", repoResult[0].Recipient);
            Assert.Equal("Staff1", repoResult[0].DispatchedByName);
        }

        [Fact]
        public void Constructor_WithNullDependencies_ThrowsArgumentNullException()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() => new ReportService(null!, _mockLookupRepository, _mockMapper));
            Assert.Throws<ArgumentNullException>(() => new ReportService(_mockReportRepository, null!, _mockMapper));
        }
    }
}
