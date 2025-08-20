using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.AuditLogServiceTest
{
    public class GetSamplLogsAsyncTests
    {
        private readonly IAuditRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly AuditLogService _service;

        public GetSamplLogsAsyncTests()
        {
            _mockRepository = Substitute.For<IAuditRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _service = new AuditLogService(_mockRepository, _mockMapper);
        }

        [Fact]
        public async Task GetSamplLogsAsync_ValidInput_ReturnsExpectedResult()
        {
            // Arrange
            var avNumber = "AV001";
            var dateFrom = DateTime.Now.AddDays(-7);
            var dateTo = DateTime.Now;
            var userId = "user123";

            var repositoryResult = new List<AuditSampleLog> { new AuditSampleLog(), new AuditSampleLog() };
            var expectedResult = new List<AuditSampleLogDTO> { new AuditSampleLogDTO(), new AuditSampleLogDTO() };

            _mockRepository.GetSamplLogsAsync(avNumber, dateFrom, dateTo, userId).Returns(repositoryResult);
            _mockMapper.Map<IEnumerable<AuditSampleLogDTO>>(repositoryResult).Returns(expectedResult);

            // Act
            var result = await _service.GetSamplLogsAsync(avNumber, dateFrom, dateTo, userId);

            // Assert
            Assert.Equal(expectedResult, result);
            await _mockRepository.Received(1).GetSamplLogsAsync(avNumber, dateFrom, dateTo, userId);
            _mockMapper.Received(1).Map<IEnumerable<AuditSampleLogDTO>>(repositoryResult);
        }

        [Theory]
        [InlineData(null, null, null, "user1")]
        [InlineData("AV123", null, null, null)]
        [InlineData(null, null, null, null)]
        public async Task GetSamplLogsAsync_NullInputParameters_ReturnsEmptyResult(
            string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid)
        {

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.GetSamplLogsAsync(avNumber, dateFrom, dateTo, userid));
        }

        [Fact]
        public async Task GetSamplLogsAsync_DifferentDateRanges_CallsRepositoryWithCorrectParameters()
        {
            // Arrange
            var avNumber = "AV002";
            var dateFrom = DateTime.Now.AddMonths(-1);
            var dateTo = DateTime.Now;
            var userId = "user456";

            // Act
            await _service.GetSamplLogsAsync(avNumber, dateFrom, dateTo, userId);

            // Assert
            await _mockRepository.Received(1).GetSamplLogsAsync(avNumber, dateFrom, dateTo, userId);
        }

        [Fact]
        public async Task GetSamplLogsAsync_DifferentUserIds_CallsRepositoryWithCorrectParameters()
        {
            // Arrange
            var avNumber = "AV003";
            var dateFrom = DateTime.Now.AddDays(-1);
            var dateTo = DateTime.Now;
            var userId1 = "user789";
            var userId2 = "user101112";

            // Act
            await _service.GetSamplLogsAsync(avNumber, dateFrom, dateTo, userId1);
            await _service.GetSamplLogsAsync(avNumber, dateFrom, dateTo, userId2);

            // Assert
            await _mockRepository.Received(1).GetSamplLogsAsync(avNumber, dateFrom, dateTo, userId1);
            await _mockRepository.Received(1).GetSamplLogsAsync(avNumber, dateFrom, dateTo, userId2);
        }

        [Fact]
        public async Task GetSamplLogsAsync_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            var avNumber = "AV004";
            var exception = new Exception("Repository error");

            _mockRepository.GetSamplLogsAsync(Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<string>())
            .Throws(exception);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetSamplLogsAsync(avNumber, null, null, string.Empty));
        }
    }
}
