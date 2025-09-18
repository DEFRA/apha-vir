using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.LookupServiceTest
{
    public class GetAllVirusFamiliesAsyncTests
    {
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _mockLookupService;

        public GetAllVirusFamiliesAsyncTests()
        {
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockLookupService = new LookupService(_mockLookupRepository, _mockMapper);
        }

        [Fact]
        public async Task GetAllVirusFamiliesAsync_ShouldReturnMappedVirusFamilies_WhenRepositoryReturnsData()
        {
            // Arrange
            var virusFamilies = new List<LookupItem>
            {
                new LookupItem { Id = Guid.NewGuid(), Name = "Family1" },
                new LookupItem { Id = Guid.NewGuid(), Name = "Family2" }
            };
            var expectedDtos = new List<LookupItemDto>
            {
                new LookupItemDto { Id = Guid.NewGuid(), Name = "Family1" },
                new LookupItemDto { Id = Guid.NewGuid(), Name = "Family2" }
            };

            _mockLookupRepository.GetAllVirusFamiliesAsync().Returns(virusFamilies);
            _mockMapper.Map<IEnumerable<LookupItemDto>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(expectedDtos);

            // Act
            var result = await _mockLookupService.GetAllVirusFamiliesAsync();

            // Assert
            Assert.Equal(expectedDtos, result);
            await _mockLookupRepository.Received(1).GetAllVirusFamiliesAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDto>>(Arg.Is<IEnumerable<LookupItem>>(x => x == virusFamilies));
        }

        [Fact]
        public async Task GetAllVirusFamiliesAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange
            _mockLookupRepository.GetAllVirusFamiliesAsync().Throws(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllVirusFamiliesAsync());
        }

        [Fact]
        public async Task GetAllVirusFamiliesAsync_ShouldReturnEmptyList_WhenRepositoryReturnsEmptyList()
        {
            // Arrange
            var emptyList = new List<LookupItem>();
            var emptyDtoList = new List<LookupItemDto>();

            _mockLookupRepository.GetAllVirusFamiliesAsync().Returns(emptyList);
            _mockMapper.Map<IEnumerable<LookupItemDto>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(emptyDtoList);

            // Act
            var result = await _mockLookupService.GetAllVirusFamiliesAsync();

            // Assert
            Assert.Empty(result);
            await _mockLookupRepository.Received(1).GetAllVirusFamiliesAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDto>>(Arg.Is<IEnumerable<LookupItem>>(x => x == emptyList));
        }

        [Fact]
        public async Task GetAllVirusFamiliesAsync_ShouldReturnCorrectType()
        {
            // Arrange
            _mockLookupRepository.GetAllVirusFamiliesAsync().Returns(new List<LookupItem>());
            _mockMapper.Map<IEnumerable<LookupItemDto>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(new List<LookupItemDto>());

            // Act
            var result = await _mockLookupService.GetAllVirusFamiliesAsync();

            // Assert
            Assert.IsAssignableFrom<IEnumerable<LookupItemDto>>(result);
        }
    }
}
