using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;

namespace Apha.VIR.Application.UnitTests.Services.AuditLogServiceTest
{
    public class GetIsolatLogDetailAsyncTests
    {
        private readonly IAuditRepository _mockAuditRepository;
        private readonly IMapper _mockMapper;
        private readonly AuditLogService _auditLogService;

        public GetIsolatLogDetailAsyncTests()
        {
            _mockAuditRepository = Substitute.For<IAuditRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _auditLogService = new AuditLogService(_mockAuditRepository, _mockMapper);
        }

        [Fact]
        public async Task GetIsolatLogDetailAsync_SuccessfulRetrieval_ReturnsExpectedDTO()
        {
            // Arrange
            var logId = Guid.NewGuid();
            var expectedResult = new List<AuditIsolateLogDetail> { new AuditIsolateLogDetail() };
            var expectedDto = new AuditIsolateLogDetailDTO();

            _mockAuditRepository.GetIsolatLogDetailAsync(logId).Returns(expectedResult);
            _mockMapper.Map<AuditIsolateLogDetailDTO>(expectedResult.FirstOrDefault()).Returns(expectedDto);

            // Act
            var result = await _auditLogService.GetIsolatLogDetailAsync(logId);

            // Assert
            Assert.Equal(expectedDto, result);
            await _mockAuditRepository.Received(1).GetIsolatLogDetailAsync(logId);
            _mockMapper.Received(1).Map<AuditIsolateLogDetailDTO>(expectedResult.FirstOrDefault());
        }

        [Fact]
        public async Task GetIsolatLogDetailAsync_EmptyResult_ReturnsNull()
        {
            // Arrange
            var logId = Guid.NewGuid();
            var emptyResult = new List<AuditIsolateLogDetail>();

            _mockAuditRepository.GetIsolatLogDetailAsync(logId).Returns(emptyResult);

            // Act
            var result = await _auditLogService.GetIsolatLogDetailAsync(logId);

            // Assert
            Assert.Null(result);
            await _mockAuditRepository.Received(1).GetIsolatLogDetailAsync(logId);

            _mockMapper.Received(1).Map<AuditIsolateLogDetailDTO>(null);
        }

        [Fact]
        public async Task GetIsolatLogDetailAsync_InvalidGuid_ReturnsNull()
        {
            // Arrange
            var invalidGuid = Guid.Empty;
            _mockAuditRepository.GetIsolatLogDetailAsync(invalidGuid).Returns(new List<AuditIsolateLogDetail>());

            // Act
            var result = await _auditLogService.GetIsolatLogDetailAsync(invalidGuid);

            // Assert
            Assert.Null(result);
            await _mockAuditRepository.Received(1).GetIsolatLogDetailAsync(invalidGuid);
            _mockMapper.Received(1).Map<AuditIsolateLogDetailDTO>(null);
        }
    }
}
