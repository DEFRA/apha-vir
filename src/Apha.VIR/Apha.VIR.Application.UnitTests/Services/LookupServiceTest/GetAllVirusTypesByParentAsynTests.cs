using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.LookupServiceTest
{
    public class GetAllVirusTypesByParentAsynTests
    {
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _mockLookupService;

        public GetAllVirusTypesByParentAsynTests()
        {
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockLookupService = new LookupService(_mockLookupRepository, _mockMapper);
        }

        [Fact]
        public async Task GetAllVirusTypesByParentAsync_WithNullVirusFamily_ReturnsExpectedResult()
        {
            // Arrange
            Guid? virusFamily = null;
            var repositoryResult = new List<LookupItem>();
            var expectedResult = new List<LookupItemDTO>();

            _mockLookupRepository.GetAllVirusTypesByParentAsync(virusFamily).Returns(repositoryResult);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(expectedResult);

            // Act
            var result = await _mockLookupService.GetAllVirusTypesByParentAsync(virusFamily);

            // Assert
            await _mockLookupRepository.Received(1).GetAllVirusTypesByParentAsync(virusFamily);
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(repositoryResult);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetAllVirusTypesByParentAsync_WithEmptyVirusFamily_ReturnsExpectedResult()
        {
            // Arrange
            Guid? virusFamily = null;
            var repositoryResult = new List<LookupItem>();
            var expectedResult = new List<LookupItemDTO>();

            _mockLookupRepository.GetAllVirusTypesByParentAsync(virusFamily).Returns(repositoryResult);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(expectedResult);

            // Act
            var result = await _mockLookupService.GetAllVirusTypesByParentAsync(virusFamily);

            // Assert
            await _mockLookupRepository.Received(1).GetAllVirusTypesByParentAsync(virusFamily);
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(repositoryResult);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetAllVirusTypesByParentAsync_WhenRepositoryReturnsEmptyList_ReturnsEmptyList()
        {
            // Arrange
            Guid virusFamily = Guid.NewGuid();
            var emptyList = new List<LookupItem>();
            var emptyDTOList = new List<LookupItemDTO>();

            _mockLookupRepository.GetAllVirusTypesByParentAsync(virusFamily).Returns(emptyList);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(emptyList).Returns(emptyDTOList);

            // Act
            var result = await _mockLookupService.GetAllVirusTypesByParentAsync(virusFamily);

            // Assert
            await _mockLookupRepository.Received(1).GetAllVirusTypesByParentAsync(virusFamily);
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(emptyList);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllVirusTypesByParentAsync_WhenRepositoryThrowsException_ThrowsException()
        {
            // Arrange
            Guid virusFamily = Guid.NewGuid();
            var expectedException = new Exception("Test exception");

            _mockLookupRepository.GetAllVirusTypesByParentAsync(virusFamily).Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllVirusTypesByParentAsync(virusFamily));
            Assert.Equal(expectedException.Message, exception.Message);
            await _mockLookupRepository.Received(1).GetAllVirusTypesByParentAsync(virusFamily);
            _mockMapper.DidNotReceive().Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>());
        }
    }
}
