using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;

namespace Apha.VIR.Application.UnitTests.Services.LookupServiceTest
{
    public class DeleteLookupItemAsyncTests
    {
        private readonly LookupService _lookupService;
        private readonly ILookupRepository _mockRepository;
        private readonly IMapper _mockMapper;

        public DeleteLookupItemAsyncTests()
        {
            _mockRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _lookupService = new LookupService(_mockRepository, _mockMapper);
        }

        [Fact]
        public async Task DeleteLookupItemAsync_SuccessfulDeletion_WhenValidInput()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var LookupItemDto = new LookupItemDto { Id = Guid.NewGuid(), Name = "Test Item" };
            var lookupItem = new LookupItem { Id = LookupItemDto.Id, Name = LookupItemDto.Name };

            _mockMapper.Map<LookupItem>(LookupItemDto).Returns(lookupItem);

            // Act
            await _lookupService.DeleteLookupItemAsync(lookupId, LookupItemDto);

            // Assert
            await _mockRepository.Received(1).DeleteLookupItemAsync(lookupId, Arg.Is<LookupItem>(item =>
            item.Id == lookupItem.Id && item.Name == lookupItem.Name));
        }

        [Fact]
        public async Task DeleteLookupItemAsync_ShouldThrowException_WhenRepositoryThrownException()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var LookupItemDto = new LookupItemDto { Id = Guid.NewGuid(), Name = "Test Item" };
            var lookupItem = new LookupItem { Id = LookupItemDto.Id, Name = LookupItemDto.Name };

            _mockMapper.Map<LookupItem>(LookupItemDto).Returns(lookupItem);
            _mockRepository.DeleteLookupItemAsync(Arg.Any<Guid>(), Arg.Any<LookupItem>())
            .Returns(Task.FromException(new Exception("Test exception")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _lookupService.DeleteLookupItemAsync(lookupId, LookupItemDto));
        }
    }
}
