using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Repositories;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.IsolatesServiceTest
{
    public class GetIsolateInfoTests
    {
        private readonly IIsolateRepository _mockIsolateRepository;
        private readonly ISubmissionRepository _mockSubmissionRepository;
        private readonly ISampleRepository _mockSampleRepository;
        private readonly ICharacteristicRepository _mockCharacteristicRepository;
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly IsolatesService _mockIsolatesService;

        public GetIsolateInfoTests()
        {
            _mockIsolateRepository = Substitute.For<IIsolateRepository>();
            _mockSubmissionRepository = Substitute.For<ISubmissionRepository>();
            _mockSampleRepository = Substitute.For<ISampleRepository>();
            _mockCharacteristicRepository = Substitute.For<ICharacteristicRepository>();
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockIsolatesService = new IsolatesService(_mockIsolateRepository, 
                _mockSubmissionRepository, 
                _mockSampleRepository, 
                _mockCharacteristicRepository, 
                _mockLookupRepository, 
                _mockMapper);
        }

        [Fact]
        public async Task GetIsolateInfoByAVNumberAsync_ReturnsIsolateInfoDTOs_WhenIsolatesExist()
        {
            // Arrange
            var avNumber = "AV123";
            var isolates = new List<IsolateInfo> { new IsolateInfo(), new IsolateInfo() };
            var isolateDtos = new List<IsolateInfoDTO> { new IsolateInfoDTO(), new IsolateInfoDTO() };
            var characteristics = new List<IsolateCharacteristicInfo> { new IsolateCharacteristicInfo() };

            _mockIsolateRepository.GetIsolateInfoByAVNumberAsync(avNumber).Returns(isolates);
            _mockMapper.Map<IEnumerable<IsolateInfoDTO>>(isolates).Returns(isolateDtos);
            _mockCharacteristicRepository.GetIsolateCharacteristicInfoAsync(Arg.Any<Guid>()).Returns(characteristics);

            // Act
            var result = await _mockIsolatesService.GetIsolateInfoByAVNumberAsync(avNumber);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            foreach (var isolate in result)
            {
                Assert.NotNull(isolate.Characteristics);
            }
        }

        [Fact]
        public async Task GetIsolateInfoByAVNumberAsync_ReturnsEmptyList_WhenNoIsolatesFound()
        {
            // Arrange
            var avNumber = "AV123";
            _mockIsolateRepository.GetIsolateInfoByAVNumberAsync(avNumber).Returns(new List<IsolateInfo>());

            // Act
            var result = await _mockIsolatesService.GetIsolateInfoByAVNumberAsync(avNumber);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetIsolateInfoByAVNumberAsync_ThrowsException_WhenRepositoryThrowsException()
        {
            // Arrange
            var avNumber = "AV123";
            _mockIsolateRepository.GetIsolateInfoByAVNumberAsync(avNumber).ThrowsAsync(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockIsolatesService.GetIsolateInfoByAVNumberAsync(avNumber));
        }
    }
}
