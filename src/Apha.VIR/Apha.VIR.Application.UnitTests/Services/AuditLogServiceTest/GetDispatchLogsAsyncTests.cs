using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.AuditLogServiceTest
{
    public class GetDispatchLogsAsyncTests
    {
        private readonly IAuditRepository _mockAuditRepository;
        private readonly IMapper _mockMapper;
        private readonly AuditLogService _auditLogService;

        public GetDispatchLogsAsyncTests()
        {
            _mockAuditRepository = Substitute.For<IAuditRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _auditLogService = new AuditLogService(_mockAuditRepository, _mockMapper);
        }

        [Fact]
        public async Task GetDispatchLogsAsync_ValidInput_ReturnsExpectedResult()
        {
            // Arrange
            string avNumber = "AV001";
            DateTime dateFrom = DateTime.Now.AddDays(-7);
            DateTime dateTo = DateTime.Now;
            string userId = "user123";

            var logId1 = Guid.NewGuid();
            var logId2 = Guid.NewGuid();

            var repositoryResult = new List<AuditDispatchLog> { new AuditDispatchLog { LogId = logId1 }, new AuditDispatchLog { LogId = logId2 } };
            var expectedResult = new List<AuditDispatchLogDto> { new AuditDispatchLogDto { LogId = logId1 }, new AuditDispatchLogDto { LogId = logId2 } };

            _mockAuditRepository.GetDispatchLogsAsync(avNumber, dateFrom, dateTo, userId)
            .Returns(repositoryResult);
            _mockMapper.Map<IEnumerable<AuditDispatchLogDto>>(repositoryResult)
            .Returns(expectedResult);

            // Act
            var result = await _auditLogService.GetDispatchLogsAsync(avNumber, dateFrom, dateTo, userId);

            // Assert
            await _mockAuditRepository.Received(1).GetDispatchLogsAsync(avNumber, dateFrom, dateTo, userId);
            _mockMapper.Received(1).Map<IEnumerable<AuditDispatchLogDto>>(repositoryResult);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(null, null, null, "user1")]
        [InlineData("AV123", null, null, null)]
        [InlineData(null, null, null, null)]
        public async Task GetDispatchLogsAsync_NullInputs_ThrowsArgumentNullException(
            string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _auditLogService.GetDispatchLogsAsync(avNumber, dateFrom, dateTo, userid));
        }

        [Fact]
        public async Task GetDispatchLogsAsync_DifferentDateRanges_CallsRepositoryWithCorrectDates()
        {
            // Arrange
            string avNumber = "AV001";
            DateTime dateFrom = DateTime.Now.AddMonths(-1);
            DateTime dateTo = DateTime.Now;
            string userId = "user123";

            _mockAuditRepository.GetDispatchLogsAsync(avNumber, dateFrom, dateTo, userId)
            .Returns(new List<AuditDispatchLog>());

            // Act
            await _auditLogService.GetDispatchLogsAsync(avNumber, dateFrom, dateTo, userId);

            // Assert
            await _mockAuditRepository.Received(1).GetDispatchLogsAsync(avNumber, dateFrom, dateTo, userId);
        }

        [Fact]
        public async Task GetDispatchLogsAsync_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            var avNumber = "AV123";
            var dateFrom = DateTime.Now.AddDays(-7);
            var dateTo = DateTime.Now;
            var userid = "user1";

            _mockAuditRepository.GetDispatchLogsAsync(avNumber, dateFrom, dateTo, userid)
            .Throws(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
            _auditLogService.GetDispatchLogsAsync(avNumber, dateFrom, dateTo, userid));
        }
    }
}
