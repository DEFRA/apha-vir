using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.AuditLogServiceTest
{
    public class GetSubmissionLogsAsynTests
    {
        private readonly IAuditRepository _mockAuditRepository;
        private readonly IMapper _mockMapper;
        private readonly AuditLogService _auditLogService;

        public GetSubmissionLogsAsynTests()
        {
            _mockAuditRepository = Substitute.For<IAuditRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _auditLogService = new AuditLogService(_mockAuditRepository, _mockMapper);
        }


        [Fact]
        public async Task GetSubmissionLogsAsync_ValidParameters_ReturnsExpectedResult()
        {
            // Arrange
            var avNumber = "AV001";
            var dateFrom = DateTime.Now.AddDays(-7);
            var dateTo = DateTime.Now;
            var userid = "user123";

            var repositoryResult = new List<AuditSubmissionLog>
            {
                new AuditSubmissionLog { LogID = Guid.NewGuid(), AVNumber = avNumber }
            };

            var expectedDtos = new List<AuditSubmissionLogDto>
            {
                new AuditSubmissionLogDto { LogID = Guid.NewGuid(), AVNumber = avNumber }
            };

            _mockAuditRepository.GetSubmissionLogsAsync(avNumber, dateFrom, dateTo, userid)
            .Returns(repositoryResult);
            _mockMapper.Map<IEnumerable<AuditSubmissionLogDto>>(Arg.Any<IEnumerable<AuditSubmissionLog>>())
            .Returns(expectedDtos);

            // Act
            var result = await _auditLogService.GetSubmissionLogsAsync(avNumber, dateFrom, dateTo, userid);

            // Assert
            Assert.Equal(expectedDtos, result);
            await _mockAuditRepository.Received(1).GetSubmissionLogsAsync(avNumber, dateFrom, dateTo, userid);
            _mockMapper.Received(1).Map<IEnumerable<AuditSubmissionLogDto>>(Arg.Any<IEnumerable<AuditSubmissionLog>>());
        }

        [Fact]
        public async Task GetSubmissionLogsAsync_NullParameters_ReturnsEmptyResult()
        {
            // Arrange
            string avNumber = null!;
            DateTime? dateFrom = null;
            DateTime? dateTo = null;
            string userid = null!;

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                 _auditLogService.GetSubmissionLogsAsync(avNumber, dateFrom, dateTo, userid));
        }

        [Fact]
        public async Task GetSubmissionLogsAsync_DifferentDateRanges_CallsRepositoryWithCorrectParameters()
        {
            // Arrange
            var avNumber = "AV002";
            var dateFrom = DateTime.Now.AddMonths(-1);
            var dateTo = DateTime.Now;
            var userid = "user456";

            _mockAuditRepository.GetSubmissionLogsAsync(avNumber, dateFrom, dateTo, userid)
            .Returns(new List<AuditSubmissionLog>());
            _mockMapper.Map<IEnumerable<AuditSubmissionLogDto>>(Arg.Any<IEnumerable<AuditSubmissionLog>>())
            .Returns(new List<AuditSubmissionLogDto>());

            // Act
            await _auditLogService.GetSubmissionLogsAsync(avNumber, dateFrom, dateTo, userid);

            // Assert
            await _mockAuditRepository.Received(1).GetSubmissionLogsAsync(avNumber, dateFrom, dateTo, userid);
        }

        [Fact]
        public async Task GetSubmissionLogsAsync_DifferentUserIds_CallsRepositoryWithCorrectParameters()
        {
            // Arrange
            var avNumber = "AV003";
            var dateFrom = DateTime.Now.AddDays(-14);
            var dateTo = DateTime.Now;
            var userid = "admin789";

            _mockAuditRepository.GetSubmissionLogsAsync(avNumber, dateFrom, dateTo, userid)
            .Returns(new List<AuditSubmissionLog>());
            _mockMapper.Map<IEnumerable<AuditSubmissionLogDto>>(Arg.Any<IEnumerable<AuditSubmissionLog>>())
            .Returns(new List<AuditSubmissionLogDto>());

            // Act
            await _auditLogService.GetSubmissionLogsAsync(avNumber, dateFrom, dateTo, userid);

            // Assert
            await _mockAuditRepository.Received(1).GetSubmissionLogsAsync(avNumber, dateFrom, dateTo, userid);
        }

        [Fact]
        public async Task GetSubmissionLogsAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var avNumber = "AV004";
            var dateFrom = DateTime.Now.AddDays(-1);
            var dateTo = DateTime.Now;
            var userid = "user999";

            _mockAuditRepository.GetSubmissionLogsAsync(avNumber, dateFrom, dateTo, userid)
            .Throws(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
            _auditLogService.GetSubmissionLogsAsync(avNumber, dateFrom, dateTo, userid));
        }
    }
}
