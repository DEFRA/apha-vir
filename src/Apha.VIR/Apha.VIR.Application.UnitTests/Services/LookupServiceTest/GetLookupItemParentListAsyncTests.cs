using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.LookupServiceTest
{
    public class GetLookupItemParentListAsyncTests
    {
        private readonly ILookupRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _lookupService;

        public GetLookupItemParentListAsyncTests()
        {
            _mockRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _lookupService = new LookupService(_mockRepository, _mockMapper);
        }

        [Fact]
        public async Task GetLookupItemParentListAsync_ShouldReturnMappedResult()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var repositoryResult = new List<LookupItem> { new LookupItem(), new LookupItem() };
            var expectedResult = new List<LookupItemDto> { new LookupItemDto(), new LookupItemDto() };

            _mockRepository.GetLookupItemParentListAsync(lookupId).Returns(repositoryResult);
            _mockMapper.Map<IEnumerable<LookupItemDto>>(repositoryResult).Returns(expectedResult);

            // Act
            var result = await _lookupService.GetLookupItemParentListAsync(lookupId);

            // Assert
            await _mockRepository.Received(1).GetLookupItemParentListAsync(lookupId);
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDto>>(repositoryResult);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetLookupItemParentListAsync_ReturnsEmptyList_WhenRepositoryReturnsEmptyList()
        {
            // Arrange
            var lookupId = Guid.Empty;

            _mockRepository.GetLookupItemParentListAsync(lookupId)
                         .Returns(Task.FromResult<IEnumerable<LookupItem>>(new List<LookupItem>()));

            _mockMapper.Map<IEnumerable<LookupItemDto>>(Arg.Any<IEnumerable<LookupItem>>())
                .Returns(new List<LookupItemDto>());


            // Act
            var result = await _lookupService.GetLookupItemParentListAsync(lookupId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetLookupItemParentListAsync_ThrowsException_WhenRepositoryThrowsException()
        {
            // Arrange
            Guid lookupId = Guid.NewGuid();
            var expectedException = new Exception("Repository error");

            _mockRepository.GetLookupItemParentListAsync(lookupId).Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _lookupService.GetLookupItemParentListAsync(lookupId));
            Assert.Equal(expectedException.Message, exception.Message);
            await _mockRepository.Received(1).GetLookupItemParentListAsync(lookupId);
        }
    }
}
