using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.IsolatesServiceTest
{
    public class IsolatesServiceTests
    {
        private readonly IIsolateRepository _mockIsolateRepository;
        private readonly ICharacteristicRepository _mockCharacteristicRepository;
        private readonly IMapper _mockMapper;
        private readonly IsolatesService _isolatesService;

        public IsolatesServiceTests()
        {
            _mockIsolateRepository = Substitute.For<IIsolateRepository>();
            _mockCharacteristicRepository = Substitute.For<ICharacteristicRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _isolatesService = new IsolatesService(_mockIsolateRepository, _mockCharacteristicRepository, _mockMapper);            
        }

        [Fact]
        public async Task GetIsolateFullDetailsAsync_SuccessfulRetrieval_ReturnsIsolateFullDetailDTO()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            var isolateFullDetail = new IsolateFullDetail
            {
                IsolateDetails = new IsolateInfo { IsolateId = isolateId },
                IsolateViabilityDetails = new List<IsolateViabilityInfo>(),
                IsolateDispatchDetails = new List<IsolateDispatchInfo>(),
                IsolateCharacteristicDetails = new List<IsolateCharacteristicInfo>()
            };
            var expectedDto = new IsolateFullDetailDTO
            {
                IsolateDetails = new IsolateInfoDTO { IsolateId = isolateId },
                IsolateViabilityDetails = new List<IsolateViabilityInfoDTO>(),
                IsolateDispatchDetails = new List<IsolateDispatchInfoDTO>(),
                IsolateCharacteristicDetails = new List<IsolateCharacteristicInfoDTO>()
            };

            _mockIsolateRepository.GetIsolateFullDetailsByIdAsync(isolateId).Returns(isolateFullDetail);
            _mockMapper.Map<IsolateFullDetailDTO>(isolateFullDetail).Returns(expectedDto);

            // Act
            var result = await _isolatesService.GetIsolateFullDetailsAsync(isolateId);

            // Assert
            Assert.Equal(expectedDto, result);
            await _mockIsolateRepository.Received(1).GetIsolateFullDetailsByIdAsync(isolateId);
            _mockMapper.Received(1).Map<IsolateFullDetailDTO>(isolateFullDetail);
        }

        [Fact]
        public async Task GetIsolateFullDetailsAsync_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            var expectedException = new Exception("Repository error");
            _mockIsolateRepository.GetIsolateFullDetailsByIdAsync(isolateId).Throws(expectedException);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _isolatesService.GetIsolateFullDetailsAsync(isolateId));
            await _mockIsolateRepository.Received(1).GetIsolateFullDetailsByIdAsync(isolateId);
            _mockMapper.DidNotReceive().Map<IsolateFullDetailDTO>(Arg.Any<IsolateFullDetail>());
        }

        [Fact]
        public async Task UpdateIsolateCharacteristicsAsync_SuccessfulUpdate_ReturnsCompletedTask()
        {
            // Arrange
            var isolateCharacteristicInfoDTO = new IsolateCharacteristicInfoDTO();
            var isolateCharacteristicInfo = new IsolateCharacteristicInfo();
            var user = "TestUser";

            _mockMapper.Map<IsolateCharacteristicInfo>(isolateCharacteristicInfoDTO).Returns(isolateCharacteristicInfo);
            _mockCharacteristicRepository.UpdateIsolateCharacteristicsAsync(isolateCharacteristicInfo, user).Returns(Task.CompletedTask);

            // Act
            await _isolatesService.UpdateIsolateCharacteristicsAsync(isolateCharacteristicInfoDTO, user);

            // Assert
            await _mockCharacteristicRepository.Received(1).UpdateIsolateCharacteristicsAsync(isolateCharacteristicInfo, user);
        }

        [Fact]
        public async Task UpdateIsolateCharacteristicsAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var isolateCharacteristicInfoDTO = new IsolateCharacteristicInfoDTO();
            var isolateCharacteristicInfo = new IsolateCharacteristicInfo();
            var user = "TestUser";
            var expectedException = new Exception("Test exception");

            _mockMapper.Map<IsolateCharacteristicInfo>(isolateCharacteristicInfoDTO).Returns(isolateCharacteristicInfo);
            _mockCharacteristicRepository.UpdateIsolateCharacteristicsAsync(isolateCharacteristicInfo, user).Throws(expectedException);

            // Act & Assert
            var actualException = await Assert.ThrowsAsync<Exception>(() => _isolatesService.UpdateIsolateCharacteristicsAsync(isolateCharacteristicInfoDTO, user));
            Assert.Same(expectedException, actualException);
        }
    }
}
