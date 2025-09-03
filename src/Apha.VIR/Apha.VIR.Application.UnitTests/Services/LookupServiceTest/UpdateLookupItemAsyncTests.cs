using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;

namespace Apha.VIR.Application.UnitTests.Services.LookupServiceTest
{
    public class UpdateLookupItemAsyncTests
    {
        private readonly ILookupRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _lookupService;

        public UpdateLookupItemAsyncTests()
        {
            _mockRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _lookupService = new LookupService(_mockRepository, _mockMapper);
        }

        [Fact]
        public async Task UpdateLookupItemAsynct_SuccessfulUpdate_WhenValidInpu()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var lookupItemDto = new LookupItemDTO { Id = Guid.NewGuid(), Name = "Test Item" };
            var lookupItem = new LookupItem { Id = lookupItemDto.Id, Name = lookupItemDto.Name };

            _mockMapper.Map<LookupItem>(lookupItemDto).Returns(lookupItem);

            // Act
            await _lookupService.UpdateLookupItemAsync(lookupId, lookupItemDto);

            // Assert
            await _mockRepository.Received(1).UpdateLookupItemAsync(lookupId, Arg.Is<LookupItem>(i => i.Id == lookupItemDto.Id && i.Name == lookupItemDto.Name));
        }

        [Fact]
        public async Task UpdateLookupItemAsync_NonExistentItem_ThrowException()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var lookupItemDto = new LookupItemDTO { Id = Guid.NewGuid(), Name = "Non-existent Item" };
            var lookupItem = new LookupItem { Id = lookupItemDto.Id, Name = lookupItemDto.Name };

            _mockMapper.Map<LookupItem>(lookupItemDto).Returns(lookupItem);
            _mockRepository.UpdateLookupItemAsync(lookupId, Arg.Any<LookupItem>()).Returns(Task.FromException(new InvalidOperationException("Item not found")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _lookupService.UpdateLookupItemAsync(lookupId, lookupItemDto));
        }

        [Fact]
        public async Task UpdateLookupItemAsync_RepositoryThrowsException_ExceptionIsThrown()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var dto = new LookupItemDTO();
            var entity = new LookupItem();

            _mockMapper.Map<LookupItem>(dto).Returns(entity);
            _mockRepository
                .UpdateLookupItemAsync(lookupId, entity)
                .Returns<Task>(_ => throw new InvalidOperationException("Update failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _lookupService.UpdateLookupItemAsync(lookupId, dto));

            Assert.Equal("Update failed", exception.Message);
            _mockMapper.Received(1).Map<LookupItem>(dto);
            await _mockRepository.Received(1).UpdateLookupItemAsync(lookupId, entity);
        }
    }
}
