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
        public async Task GetAllLookupItemsAsync_SuccessfulRetrieval_ReturnsLookupItems()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var lookupItems = new List<LookupItem>
            {
                new LookupItem { Id = Guid.NewGuid(), Name = "Item1" },
                new LookupItem { Id = Guid.NewGuid(), Name = "Item2" }
            };

            var pagedRepoResult = new PagedData<LookupItem>(lookupItems, 10);

            var expectedDtos = new List<LookupItemDTO>
            {
            new LookupItemDTO { Id = lookupItems[0].Id, Name = lookupItems[0].Name },
            new LookupItemDTO { Id = lookupItems[1].Id, Name = lookupItems[1].Name }
            };

            var pagedexpectedResult = new PaginatedResult<LookupItemDTO>(expectedDtos, 10);


            _mockLookupRepository.GetAllLookupItemsAsync(lookupId, 1, 10).Returns(pagedRepoResult);

            _mockMapper.Map<PaginatedResult<LookupItemDTO>>(Arg.Any<PagedData<LookupItem>>()).Returns(pagedexpectedResult);

            // Act
            var result = await _mockLookupService.GetAllLookupItemsAsync(lookupId, 1, 10);

            // Assert
            await _mockLookupRepository.Received(1).GetAllLookupItemsAsync(lookupId, 1, 10);
            _mockMapper.Received(1).Map<PaginatedResult<LookupItemDTO>>(Arg.Any<PagedData<LookupItem>>());
            Assert.Equal(pagedexpectedResult, result);
        }

        [Fact]
        public async Task GetAllLookupItemsAsync_ExceptionThrown_PropagatesException()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var expectedException = new Exception("Test exception");

            _mockLookupRepository.GetAllLookupItemsAsync(lookupId, 1, 10).Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllLookupItemsAsync(lookupId, 1, 10));
            Assert.Same(expectedException, exception);
            await _mockLookupRepository.Received(1).GetAllLookupItemsAsync(lookupId, 1, 10);
            _mockMapper.DidNotReceive().Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>());
        }

    }
}
