using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;

namespace Apha.VIR.Application.UnitTests.Services.LookupServiceTest
{
    public class GetLookupItemAsyncTests
    {
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _lookupService;

        public GetLookupItemAsyncTests()
        {
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _lookupService = new LookupService(_mockLookupRepository, _mockMapper);
        }

        [Fact]
        public async Task GetLookupItemAsync_ShouldReturnLookupItemDtoList_WhenLookupItemExists()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var lookupItemId = Guid.NewGuid();
            var lookupItem = new LookupItem { Id = lookupItemId, Name = "Test Item" };
            var expectedDto = new LookupItemDto { Id = lookupItemId, Name = "Test Item" };

            _mockLookupRepository.GetLookupItemAsync(lookupId, lookupItemId).Returns(lookupItem);
            _mockMapper.Map<LookupItemDto>(lookupItem).Returns(expectedDto);

            // Act
            var result = await _lookupService.GetLookupItemAsync(lookupId, lookupItemId);

            // Assert
            Assert.Equal(expectedDto, result);
        }

        [Fact]
        public async Task GetLookupItemAsync_ShouldCorrectlyMap_FromLookupItemToLookupItemDto()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var lookupItemId = Guid.NewGuid();
            var lookupItem = new LookupItem { Id = lookupItemId, Name = "Test Item" };
            var expectedDto = new LookupItemDto { Id = lookupItemId, Name = "Test Item" };

            _mockLookupRepository.GetLookupItemAsync(lookupId, lookupItemId).Returns(lookupItem);
            _mockMapper.Map<LookupItemDto>(lookupItem).Returns(expectedDto);

            // Act
            var result = await _lookupService.GetLookupItemAsync(lookupId, lookupItemId);

            // Assert
            _mockMapper.Received(1).Map<LookupItemDto>(lookupItem);
            Assert.Equal(expectedDto.Id, result.Id);
            Assert.Equal(expectedDto.Name, result.Name);
        }
    }
}
