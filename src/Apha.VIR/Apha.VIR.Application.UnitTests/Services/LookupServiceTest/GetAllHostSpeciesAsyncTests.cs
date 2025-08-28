using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.LookupServiceTest
{
    public class GetAllHostSpeciesAsyncTests
    {
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _mockLookupService;

        public GetAllHostSpeciesAsyncTests()
        {
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockLookupService = new LookupService(_mockLookupRepository, _mockMapper);
        }

        [Fact]
        public async Task GetAllHostSpeciesAsync_ShouldReturnExpectedResult()
        {
            // Arrange
            var hostSpecies = new List<LookupItem>
            {
                new LookupItem { Id = Guid.NewGuid(), Name = "Species 1" },
                new LookupItem { Id = Guid.NewGuid(), Name = "Species 2" }
            };
            var expectedDTOs = new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = hostSpecies[0].Id, Name = hostSpecies[0].Name },
                new LookupItemDTO { Id = hostSpecies[1].Id, Name = hostSpecies[1].Name }
            };

            _mockLookupRepository.GetAllHostSpeciesAsync().Returns(hostSpecies);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(expectedDTOs);

            // Act
            var result = await _mockLookupService.GetAllHostSpeciesAsync();

            // Assert
            Assert.Equal(expectedDTOs, result);
            await _mockLookupRepository.Received(1).GetAllHostSpeciesAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => x == hostSpecies));
        }

        [Fact]
        public async Task GetAllHostSpeciesAsync_ShouldCallRepositoryOnce()
        {
            // Arrange
            _mockLookupRepository.GetAllHostSpeciesAsync().Returns(new List<LookupItem>());

            // Act
            await _mockLookupService.GetAllHostSpeciesAsync();

            // Assert
            await _mockLookupRepository.Received(1).GetAllHostSpeciesAsync();
        }

        [Fact]
        public async Task GetAllHostSpeciesAsync_ShouldCallMapperWithCorrectParameters()
        {
            // Arrange
            var hostSpecies = new List<LookupItem>();
            _mockLookupRepository.GetAllHostSpeciesAsync().Returns(hostSpecies);

            // Act
            await _mockLookupService.GetAllHostSpeciesAsync();

            // Assert
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => x == hostSpecies));
        }

        [Fact]
        public async Task GetAllHostSpeciesAsync_ShouldReturnEmptyList_WhenRepositoryReturnsEmptyList()
        {
            // Arrange
            _mockLookupRepository.GetAllHostSpeciesAsync().Returns(new List<LookupItem>());
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(new List<LookupItemDTO>());

            // Act
            var result = await _mockLookupService.GetAllHostSpeciesAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllHostSpeciesAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange
            var expectedException = new Exception("Test exception");
            _mockLookupRepository.GetAllHostSpeciesAsync().Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllHostSpeciesAsync());
            Assert.Same(expectedException, exception);
        }
    }
}
