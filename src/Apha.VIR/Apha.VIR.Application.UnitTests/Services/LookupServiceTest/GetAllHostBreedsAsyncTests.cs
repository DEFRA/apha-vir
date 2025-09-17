using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.LookupServiceTest
{
    public class GetAllHostBreedsAsyncTests
    {
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _mockLookupService;

        public GetAllHostBreedsAsyncTests()
        {
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockLookupService = new LookupService(_mockLookupRepository, _mockMapper);
        }
        [Fact]
        public async Task GetAllHostBreedsAsync_ReturnsHostBreedList()
        {
            // Arrange
            var mockHostBreeds = new List<LookupItem>
            {
                new LookupItem { Id = Guid.NewGuid(), Name = "Breed1" },
                new LookupItem { Id = Guid.NewGuid(), Name = "Breed2" }
            };

            var expectedDTOs = new List<LookupItemDto>
            {
                new LookupItemDto { Id = mockHostBreeds[0].Id, Name = mockHostBreeds[0].Name },
                new LookupItemDto { Id = mockHostBreeds[1].Id, Name = mockHostBreeds[1].Name }
            };

            _mockLookupRepository.GetAllHostBreedsAsync().Returns(mockHostBreeds);
            _mockMapper.Map<IEnumerable<LookupItemDto>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(expectedDTOs);

            // Act
            var result = await _mockLookupService.GetAllHostBreedsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDTOs, result);
            await _mockLookupRepository.Received(1).GetAllHostBreedsAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDto>>(Arg.Is<IEnumerable<LookupItem>>(x => x == mockHostBreeds));
        }

        [Fact]
        public async Task GetAllHostBreedsAsync_ThrowsException_WhenRepositoryFails()
        {
            // Arrange
            _mockLookupRepository.GetAllHostBreedsAsync().Throws(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllHostBreedsAsync());
            await _mockLookupRepository.Received(1).GetAllHostBreedsAsync();
        }
    }
}
