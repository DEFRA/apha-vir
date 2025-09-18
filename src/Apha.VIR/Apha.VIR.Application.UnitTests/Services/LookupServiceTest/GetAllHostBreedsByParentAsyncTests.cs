using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.LookupServiceTest
{
    public class GetAllHostBreedsByParentAsyncTests
    {
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _mockLookupService;

        public GetAllHostBreedsByParentAsyncTests()
        {
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockLookupService = new LookupService(_mockLookupRepository, _mockMapper);
        }
        [Fact]
        public async Task GetAllHostBreedsByParentAsyncs_ReturnsLookupItemDtosList_WhenValidHostSpecie()
        {
            // Arrange
            Guid hostSpecies = Guid.NewGuid();
            var lookupItems = new List<LookupItem> { new LookupItem { Id = Guid.NewGuid(), Name = "Labrador" } };
            var LookupItemDtos = new List<LookupItemDto> { new LookupItemDto { Id = Guid.NewGuid(), Name = "Labrador" } };

            _mockLookupRepository.GetAllHostBreedsByParentAsync(hostSpecies).Returns(lookupItems);
            _mockMapper.Map<IEnumerable<LookupItemDto>>(lookupItems).Returns(LookupItemDtos);

            // Act
            var result = await _mockLookupService.GetAllHostBreedsByParentAsync(hostSpecies);

            // Assert
            await _mockLookupRepository.Received(1).GetAllHostBreedsByParentAsync(hostSpecies);
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDto>>(lookupItems);
            Assert.Equal(LookupItemDtos, result);
        }

        [Fact]
        public async Task GetAllHostBreedsByParentAsync_ReturnsLookupItemDtosList_WhenNullHostSpecies()
        {
            // Arrange
            Guid? hostSpecies = null;
            var lookupItems = new List<LookupItem> { new LookupItem { Id = Guid.NewGuid(), Name = "All Breeds" } };
            var LookupItemDtos = new List<LookupItemDto> { new LookupItemDto { Id = Guid.NewGuid(), Name = "All Breeds" } };

            _mockLookupRepository.GetAllHostBreedsByParentAsync(hostSpecies).Returns(lookupItems);
            _mockMapper.Map<IEnumerable<LookupItemDto>>(lookupItems).Returns(LookupItemDtos);

            // Act
            var result = await _mockLookupService.GetAllHostBreedsByParentAsync(hostSpecies);

            // Assert
            await _mockLookupRepository.Received(1).GetAllHostBreedsByParentAsync(hostSpecies);
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDto>>(lookupItems);
            Assert.Equal(LookupItemDtos, result);
        }

        [Fact]
        public async Task GetAllHostBreedsByParentAsync_ReturnsEmptyList_WhenNoPresentPresent()
        {
            // Arrange
            Guid hostSpecies = Guid.NewGuid();
            var emptyList = new List<LookupItem>();
            var emptyDTOList = new List<LookupItemDto>();

            _mockLookupRepository.GetAllHostBreedsByParentAsync(hostSpecies).Returns(emptyList);
            _mockMapper.Map<IEnumerable<LookupItemDto>>(emptyList).Returns(emptyDTOList);

            // Act
            var result = await _mockLookupService.GetAllHostBreedsByParentAsync(hostSpecies);

            // Assert
            await _mockLookupRepository.Received(1).GetAllHostBreedsByParentAsync(hostSpecies);
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDto>>(emptyList);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllHostBreedsByParentAsync_ThrowsException_WhenRepositoryThrowsException()
        {
            // Arrange
            Guid hostSpecies = Guid.NewGuid();
            var expectedException = new Exception("Repository error");

            _mockLookupRepository.GetAllHostBreedsByParentAsync(hostSpecies).Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllHostBreedsByParentAsync(hostSpecies));
            Assert.Equal(expectedException.Message, exception.Message);
            await _mockLookupRepository.Received(1).GetAllHostBreedsByParentAsync(hostSpecies);
        }
    }
}
