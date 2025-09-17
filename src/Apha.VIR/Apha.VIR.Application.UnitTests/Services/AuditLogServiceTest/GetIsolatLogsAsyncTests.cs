using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;

namespace Apha.VIR.Application.UnitTests.Services.AuditLogServiceTest
{
    public class GetIsolatLogsAsyncTests
    {
        private readonly IAuditRepository _mockAuditRepository;
        private readonly IMapper _mockMapper;
        private readonly AuditLogService _auditLogService;

        public GetIsolatLogsAsyncTests()
        {
            _mockAuditRepository = Substitute.For<IAuditRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _auditLogService = new AuditLogService(_mockAuditRepository, _mockMapper);
        }

        [Fact]
        public async Task GetIsolatLogsAsync_WithValidParameters_ReturnsExpectedResult()
        {
            // Arrange
            var avNumber = "AV123";
            var dateFrom = DateTime.Now.AddDays(-7);
            var dateTo = DateTime.Now;
            var userid = "user1";

            var repositoryResult = new List<AuditIsolateLog> { new AuditIsolateLog(), new AuditIsolateLog() };
            var expectedDtos = new List<AuditIsolateLogDto> { new AuditIsolateLogDto(), new AuditIsolateLogDto() };

            _mockAuditRepository.GetIsolatLogsAsync(avNumber, dateFrom, dateTo, userid)
            .Returns(repositoryResult);
            _mockMapper.Map<IEnumerable<AuditIsolateLogDto>>(repositoryResult)
            .Returns(expectedDtos);

            // Act
            var result = await _auditLogService.GetIsolatLogsAsync(avNumber, dateFrom, dateTo, userid);

            // Assert
            await _mockAuditRepository.Received(1).GetIsolatLogsAsync(avNumber, dateFrom, dateTo, userid);
            _mockMapper.Received(1).Map<IEnumerable<AuditIsolateLogDto>>(repositoryResult);
            Assert.Equal(expectedDtos, result);
        }

        [Fact]
        public async Task GetIsolatLogsAsync_WithNullParameters_CallsRepositoryWithNullValues()
        {
            // Arrange
            string avNumber = null!;
            DateTime? dateFrom = null;
            DateTime? dateTo = null;
            string userid = null!;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _auditLogService.GetIsolatLogsAsync(avNumber, dateFrom, dateTo, userid));
        }

        [Fact]
        public async Task GetIsolatLogsAsync_WithEmptyParameters_CallsRepositoryWithEmptyValues()
        {
            // Arrange
            var avNumber = string.Empty;
            var userid = string.Empty;

            _mockAuditRepository.GetIsolatLogsAsync(string.Empty, null, null, string.Empty)
            .Returns(new List<AuditIsolateLog>());

            // Act
            await _auditLogService.GetIsolatLogsAsync(avNumber, null, null, userid);

            // Assert
            await _mockAuditRepository.Received(1).GetIsolatLogsAsync(string.Empty, null, null, string.Empty);
        }

        [Fact]
        public async Task GetIsolatLogsAsync_WithDifferentDateRanges_CallsRepositoryWithCorrectDates()
        {
            // Arrange
            var avNumber = "AV123";
            var dateFrom = DateTime.Now.AddMonths(-1);
            var dateTo = DateTime.Now;
            var userid = "user1";

            _mockAuditRepository.GetIsolatLogsAsync(avNumber, dateFrom, dateTo, userid)
            .Returns(new List<AuditIsolateLog>());

            // Act
            await _auditLogService.GetIsolatLogsAsync(avNumber, dateFrom, dateTo, userid);

            // Assert
            await _mockAuditRepository.Received(1).GetIsolatLogsAsync(avNumber, dateFrom, dateTo, userid);
        }

        [Fact]
        public async Task GetIsolatLogsAsync_WhenRepositoryReturnsNoResults_ReturnsEmptyList()
        {
            // Arrange
            var avNumber = "AV123";
            var userid = "user1";

            _mockAuditRepository.GetIsolatLogsAsync(avNumber, null, null, userid)
            .Returns(new List<AuditIsolateLog>());
            _mockMapper.Map<IEnumerable<AuditIsolateLogDto>>(Arg.Any<IEnumerable<object>>())
            .Returns(new List<AuditIsolateLogDto>());

            // Act
            var result = await _auditLogService.GetIsolatLogsAsync(avNumber, null, null, userid);

            // Assert
            Assert.Empty(result);
        }
    }
}
