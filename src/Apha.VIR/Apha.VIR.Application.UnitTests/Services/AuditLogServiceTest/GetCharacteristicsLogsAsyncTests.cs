using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.AuditLogServiceTest
{
    public class GetCharacteristicsLogsAsyncTests
    {
        private readonly IAuditRepository _mockAuditRepository;
        private readonly IMapper _mockMapper;
        private readonly AuditLogService _auditLogService;

        public GetCharacteristicsLogsAsyncTests()
        {
            _mockAuditRepository = Substitute.For<IAuditRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _auditLogService = new AuditLogService(_mockAuditRepository, _mockMapper);
        }

        [Fact]
        public async Task GetCharacteristicsLogsAsync_ValidInput_ReturnsExpectedResult()
        {
            // Arrange
            var avNumber = "AV123";
            var dateFrom = DateTime.Now.AddDays(-7);
            var dateTo = DateTime.Now;
            var userid = "user1";

            var repositoryResult = new List<AuditCharacteristicLog>
            { new AuditCharacteristicLog { LogId = Guid.NewGuid() }};

            var expectedDtoResult = new List<AuditCharacteristicLogDTO>
            { new AuditCharacteristicLogDTO { LogId = Guid.NewGuid() }};

            _mockAuditRepository.GetCharacteristicsLogsAsync(avNumber, dateFrom, dateTo, userid)
            .Returns(repositoryResult);
            _mockMapper.Map<IEnumerable<AuditCharacteristicLogDTO>>(repositoryResult)
            .Returns(expectedDtoResult);

            // Act
            var result = await _auditLogService.GetCharacteristicsLogsAsync(avNumber, dateFrom, dateTo, userid);

            // Assert
            await _mockAuditRepository.Received(1).GetCharacteristicsLogsAsync(avNumber, dateFrom, dateTo, userid);
            _mockMapper.Received(1).Map<IEnumerable<AuditCharacteristicLogDTO>>(repositoryResult);
            Assert.Equal(expectedDtoResult, result);
        }

        [Theory]
        [InlineData("", null, null, "")]
        public async Task GetCharacteristicsLogsAsync_EmptyInput_CallsRepositoryWithCorrectParameters(
            string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid)
        {
            // Arrange
            _mockAuditRepository.GetCharacteristicsLogsAsync(avNumber, dateFrom, dateTo, userid)
            .Returns(new List<AuditCharacteristicLog>());

            // Act
            await _auditLogService.GetCharacteristicsLogsAsync(avNumber, dateFrom, dateTo, userid);

            // Assert
            await _mockAuditRepository.Received(1).GetCharacteristicsLogsAsync(avNumber, dateFrom, dateTo, userid);
        }

        [Theory]
        [InlineData(null, null, null, "user1")]
        [InlineData("AV123", null, null, null)]
        [InlineData(null, null, null, null)]
        public async Task GetCharacteristicsLogsAsync_NullInputs_ThrowsArgumentNullException(
            string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _auditLogService.GetCharacteristicsLogsAsync(avNumber, dateFrom, dateTo, userid));
        }

        [Fact]
        public async Task GetCharacteristicsLogsAsync_DifferentDateRanges_CallsRepositoryWithCorrectParameters()
        {
            // Arrange
            var avNumber = "AV123";
            var dateFrom = DateTime.Now.AddMonths(-1);
            var dateTo = DateTime.Now;
            var userid = "user1";

            _mockAuditRepository.GetCharacteristicsLogsAsync(avNumber, dateFrom, dateTo, userid)
            .Returns(new List<AuditCharacteristicLog>());

            // Act
            await _auditLogService.GetCharacteristicsLogsAsync(avNumber, dateFrom, dateTo, userid);

            // Assert
            await _mockAuditRepository.Received(1).GetCharacteristicsLogsAsync(avNumber, dateFrom, dateTo, userid);
        }

        [Theory]
        [InlineData("user1")]
        [InlineData("user2")]
        [InlineData("")]
        public async Task GetCharacteristicsLogsAsync_DifferentUserIds_CallsRepositoryWithCorrectParameters(string userid)
        {
            // Arrange
            var avNumber = "AV123";
            var dateFrom = DateTime.Now.AddDays(-7);
            var dateTo = DateTime.Now;

            _mockAuditRepository.GetCharacteristicsLogsAsync(avNumber, dateFrom, dateTo, userid)
            .Returns(new List<AuditCharacteristicLog>());

            // Act
            await _auditLogService.GetCharacteristicsLogsAsync(avNumber, dateFrom, dateTo, userid);

            // Assert
            await _mockAuditRepository.Received(1).GetCharacteristicsLogsAsync(avNumber, dateFrom, dateTo, userid);
        }

        [Fact]
        public async Task GetCharacteristicsLogsAsync_NullUserId_ThrowsArgumentNullException()
        {
            // Arrange
            var avNumber = "AV123";
            var dateFrom = DateTime.Now.AddDays(-7);
            var dateTo = DateTime.Now;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _auditLogService.GetCharacteristicsLogsAsync(avNumber, dateFrom, dateTo, null));
        }

        [Fact]
        public async Task GetCharacteristicsLogsAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var avNumber = "AV123";
            var dateFrom = DateTime.Now.AddDays(-7);
            var dateTo = DateTime.Now;
            var userid = "user1";

            _mockAuditRepository.GetCharacteristicsLogsAsync(avNumber, dateFrom, dateTo, userid)
            .Throws(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
            _auditLogService.GetCharacteristicsLogsAsync(avNumber, dateFrom, dateTo, userid));
        }
    }
}
