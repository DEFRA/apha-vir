using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.LookupServiceTest
{
    public class GetAllHostPurposesAsyncTests
    {
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _mockLookupService;
 
        public GetAllHostPurposesAsyncTests()
        {
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockLookupService = new LookupService(_mockLookupRepository, _mockMapper);
        }

        [Fact]
        public async Task GetAllHostPurposesAsync_ReturnsExpectedHostPurposesList_WhenSuccessfulRetrival()
        {
            // Arrange
            var repositoryResult = new List<LookupItem>
            {
                new LookupItem { Id = Guid.NewGuid(), Name = "Purpose 1" },
                new LookupItem { Id = Guid.NewGuid(), Name = "Purpose 2" }
            };

            var expectedResult = new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Purpose 1" },
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Purpose 2" }
            };

            _mockLookupRepository.GetAllHostPurposesAsync().Returns(repositoryResult);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(expectedResult);

            // Act
            var result = await _mockLookupService.GetAllHostPurposesAsync();

            // Assert
            Assert.Equal(expectedResult, result);
            await _mockLookupRepository.Received(1).GetAllHostPurposesAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => x == repositoryResult));
        }

        [Fact]
        public async Task GetAllHostPurposesAsync_ThrowsException_WhenRepositoryThrowsException()
        {
            // Arrange
            _mockLookupRepository.GetAllHostPurposesAsync().Throws(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllHostPurposesAsync());
            await _mockLookupRepository.Received(1).GetAllHostPurposesAsync();
        }

        [Fact]
        public async Task GetAllHostPurposesAsync_ThrowsException_WhenMapperThrowsException()
        {
            // Arrange
            var repositoryResult = new List<LookupItem>();
            _mockLookupRepository.GetAllHostPurposesAsync().Returns(repositoryResult);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Throws(new Exception("Mapper error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllHostPurposesAsync());
            await _mockLookupRepository.Received(1).GetAllHostPurposesAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => x == repositoryResult));
        }
    }
}
