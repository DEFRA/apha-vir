using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.AuditLogServiceTest
{
    public class GetIsolateViabilityLogsAsyncTests
    {
        private readonly IAuditRepository _auditRepository;
        private readonly IMapper _mapper;
        private readonly AuditLogService _auditLogService;

        public GetIsolateViabilityLogsAsyncTests()
        {
            _auditRepository = Substitute.For<IAuditRepository>();
            _mapper = Substitute.For<IMapper>();
            _auditLogService = new AuditLogService(_auditRepository, _mapper);
        }

        [Fact]
        public async Task GetIsolateViabilityLogsAsync_ValidInput_ReturnsExpectedResult()
        {
            // Arrange
            var avNumber = "AV001";
            var dateFrom = DateTime.Now.AddDays(-7);
            var dateTo = DateTime.Now;
            var userid = "user123";
            var logId1 = Guid.NewGuid();
            var logId2 = Guid.NewGuid();

            var repositoryResult = new List<AuditViabilityLog> { new AuditViabilityLog { LogId = logId1 }, new AuditViabilityLog { LogId = logId2 } };
            var expectedResult = new List<AuditViabilityLogDTO> { new AuditViabilityLogDTO { LogId = logId1 }, new AuditViabilityLogDTO { LogId = logId2 } };

            _auditRepository.GetIsolateViabilityLogsAsync(avNumber, dateFrom, dateTo, userid)
            .Returns(repositoryResult);
            _mapper.Map<IEnumerable<AuditViabilityLogDTO>>(repositoryResult).Returns(expectedResult);

            // Act
            var result = await _auditLogService.GetIsolateViabilityLogsAsync(avNumber, dateFrom, dateTo, userid);

            // Assert
            Assert.Equal(expectedResult, result);
            await _auditRepository.Received(1).GetIsolateViabilityLogsAsync(avNumber, dateFrom, dateTo, userid);
            _mapper.Received(1).Map<IEnumerable<AuditViabilityLogDTO>>(repositoryResult);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetIsolateViabilityLogsAsync_NullOrEmptyAvNumber_CallsRepositoryWithCorrectParameters(string avNumber)
        {
            // Arrange
            var dateFrom = DateTime.Now.AddDays(-7);
            var dateTo = DateTime.Now;
            var userid = "user123";

            // Act
            await _auditLogService.GetIsolateViabilityLogsAsync(avNumber, dateFrom, dateTo, userid);

            // Assert
            await _auditRepository.Received(1).GetIsolateViabilityLogsAsync(avNumber, dateFrom, dateTo, userid);
        }

        [Fact]
        public async Task GetIsolateViabilityLogsAsync_NullDates_CallsRepositoryWithCorrectParameters()
        {
            // Arrange
            var avNumber = "AV001";
            var userid = "user123";

            // Act
            await _auditLogService.GetIsolateViabilityLogsAsync(avNumber, null, null, userid);

            // Assert
            await _auditRepository.Received(1).GetIsolateViabilityLogsAsync(avNumber, null, null, userid);
        }

        [Fact]
        public async Task GetIsolateViabilityLogsAsync_InvalidDateRange_CallsRepositoryWithCorrectParameters()
        {
            // Arrange
            var avNumber = "AV001";
            var dateFrom = DateTime.Now;
            var dateTo = DateTime.Now.AddDays(-7);
            var userid = "user123";

            // Act
            await _auditLogService.GetIsolateViabilityLogsAsync(avNumber, dateFrom, dateTo, userid);

            // Assert
            await _auditRepository.Received(1).GetIsolateViabilityLogsAsync(avNumber, dateFrom, dateTo, userid);
        }

        [Theory]
        [InlineData("user123")]
        [InlineData("")]
        [InlineData(null)]
        public async Task GetIsolateViabilityLogsAsync_DifferentUserIds_CallsRepositoryWithCorrectParameters(string userid)
        {
            // Arrange
            var avNumber = "AV001";
            var dateFrom = DateTime.Now.AddDays(-7);
            var dateTo = DateTime.Now;

            // Act
            await _auditLogService.GetIsolateViabilityLogsAsync(avNumber, dateFrom, dateTo, userid);

            // Assert
            await _auditRepository.Received(1).GetIsolateViabilityLogsAsync(avNumber, dateFrom, dateTo, userid);
        }

        [Fact]
        public async Task GetIsolateViabilityLogsAsync_EmptyResult_ReturnsEmptyCollection()
        {
            // Arrange
            var avNumber = "AV001";
            var dateFrom = DateTime.Now.AddDays(-7);
            var dateTo = DateTime.Now;
            var userid = "user123";

            _auditRepository.GetIsolateViabilityLogsAsync(avNumber, dateFrom, dateTo, userid)
            .Returns(Enumerable.Empty<AuditViabilityLog>());

            _mapper.Map<IEnumerable<AuditViabilityLogDTO>>(Arg.Any<IEnumerable<object>>())
            .Returns(Enumerable.Empty<AuditViabilityLogDTO>());

            // Act
            var result = await _auditLogService.GetIsolateViabilityLogsAsync(avNumber, dateFrom, dateTo, userid);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetIsolateViabilityLogsAsync_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            var avNumber = "AV001";
            var dateFrom = DateTime.Now.AddDays(-7);
            var dateTo = DateTime.Now;
            var userid = "user123";

            _auditRepository.GetIsolateViabilityLogsAsync(avNumber, dateFrom, dateTo, userid)
            .Throws(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
            _auditLogService.GetIsolateViabilityLogsAsync(avNumber, dateFrom, dateTo, userid));
        }
    }
}
