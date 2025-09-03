using Apha.VIR.Application.Services;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;

namespace Apha.VIR.Application.UnitTests.Services.LookupServiceTest
{
    public class IsLookupItemInUseAsyncTests
    {
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _lookupService;

        public IsLookupItemInUseAsyncTests()
        {
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _lookupService = new LookupService(_mockLookupRepository, _mockMapper);
        }


        [Fact]
        public async Task IsLookupItemInUseAsync_ReturnsTrue_WhenItemIsInUse()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var lookupItemId = Guid.NewGuid();
            var expectedResult = true;

            _mockLookupRepository.IsLookupItemInUseAsync(lookupId, lookupItemId).Returns(expectedResult);

            // Act
            var result = await _lookupService.IsLookupItemInUseAsync(lookupId, lookupItemId);

            // Assert
            Assert.Equal(expectedResult, result);
            await _mockLookupRepository.Received(1).IsLookupItemInUseAsync(lookupId, lookupItemId);
        }

        [Fact]
        public async Task IsLookupItemInUseAsync_ReturnsFalse_WhenItemIsNotInUse()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var lookupItemId = Guid.NewGuid();
            _mockLookupRepository.IsLookupItemInUseAsync(lookupId, lookupItemId).Returns(false);

            // Act
            var result = await _lookupService.IsLookupItemInUseAsync(lookupId, lookupItemId);

            // Assert
            Assert.False(result);
            await _mockLookupRepository.Received(1).IsLookupItemInUseAsync(lookupId, lookupItemId);
        }
    }
}
