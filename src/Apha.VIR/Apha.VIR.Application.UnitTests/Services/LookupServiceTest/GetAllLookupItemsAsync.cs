using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Pagination;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.Core.Pagination;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.LookupServiceTest
{
    public class GetAllLookupItemsAsync
    {
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _mockLookupService;

        public GetAllLookupItemsAsync()
        {
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockLookupService = new LookupService(_mockLookupRepository, _mockMapper);
        }
        [Fact]
        public async Task GetAllLookupItemsAsync_ReturnsPagedLookupItems_WhenSuccessfulRetrieval()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var lookupItems = new List<LookupItem>
            {
                new LookupItem { Id = Guid.NewGuid(), Name = "Item1" },
                new LookupItem { Id = Guid.NewGuid(), Name = "Item2" }
            };

            var pagedRepoResult = new PagedData<LookupItem>(lookupItems, 10);

            var expectedDtos = new List<LookupItemDto>
            {
            new LookupItemDto { Id = lookupItems[0].Id, Name = lookupItems[0].Name },
            new LookupItemDto { Id = lookupItems[1].Id, Name = lookupItems[1].Name }
            };

            var pagedexpectedResult = new PaginatedResult<LookupItemDto>(expectedDtos, 10);


            _mockLookupRepository.GetAllLookupItemsAsync(lookupId, 1, 10).Returns(pagedRepoResult);

            _mockMapper.Map<PaginatedResult<LookupItemDto>>(Arg.Any<PagedData<LookupItem>>()).Returns(pagedexpectedResult);

            // Act
            var result = await _mockLookupService.GetAllLookupItemsAsync(lookupId, 1, 10);

            // Assert
            await _mockLookupRepository.Received(1).GetAllLookupItemsAsync(lookupId, 1, 10);
            _mockMapper.Received(1).Map<PaginatedResult<LookupItemDto>>(Arg.Any<PagedData<LookupItem>>());
            Assert.Equal(pagedexpectedResult, result);
        }

        [Fact]
        public async Task GetAllLookupItemsAsync_PropagatesException_WhenExceptionThrown()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var expectedException = new Exception("Test exception");

            _mockLookupRepository.GetAllLookupItemsAsync(lookupId, 1, 10).Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllLookupItemsAsync(lookupId, 1, 10));
            Assert.Same(expectedException, exception);
            await _mockLookupRepository.Received(1).GetAllLookupItemsAsync(lookupId, 1, 10);
            _mockMapper.DidNotReceive().Map<IEnumerable<LookupItemDto>>(Arg.Any<IEnumerable<LookupItem>>());
        }

        [Fact]
        public async Task GetAllLookupItemsAsync_ReturnsLookupItems_WhenSuccessfulRetrieval()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var lookupItems = new List<LookupItem> { new LookupItem(), new LookupItem() };
            var expectedDtos = new List<LookupItemDto> { new LookupItemDto(), new LookupItemDto() };

            _mockLookupRepository.GetAllLookupItemsAsync(lookupId).Returns(lookupItems);
            _mockMapper.Map<IEnumerable<LookupItemDto>>(lookupItems).Returns(expectedDtos);

            // Act
            var result = await _mockLookupService.GetAllLookupItemsAsync(lookupId);

            // Assert
            await _mockLookupRepository.Received(1).GetAllLookupItemsAsync(lookupId);
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDto>>(lookupItems);
            Assert.Equal(expectedDtos, result);
        }

        [Fact]
        public async Task GetAllLookupItemsAsync_ReturnsEmptyList_WhenNoLookitemFound()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var emptyList = new List<LookupItem>();
            var emptyDtoList = new List<LookupItemDto>();

            _mockLookupRepository.GetAllLookupItemsAsync(lookupId).Returns(emptyList);
            _mockMapper.Map<IEnumerable<LookupItemDto>>(emptyList).Returns(emptyDtoList);

            // Act
            var result = await _mockLookupService.GetAllLookupItemsAsync(lookupId);

            // Assert
            await _mockLookupRepository.Received(1).GetAllLookupItemsAsync(lookupId);
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDto>>(emptyList);
            Assert.Empty(result);
        }
    }
}
