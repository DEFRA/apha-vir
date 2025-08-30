using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;

namespace Apha.VIR.Application.UnitTests.Services.LookupServiceTest
{
    public class GetAllIsolationMethodsAsyncTests
    {
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _mockLookupService;

        public GetAllIsolationMethodsAsyncTests()
        {
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockLookupService = new LookupService(_mockLookupRepository, _mockMapper);
        }

        [Fact]
        public async Task GetAllIsolationMethodsAsync_ShouldReturnIsolationMethods()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var isolationMethods = new List<LookupItem>
            {
                new LookupItem { Id = id1, Name = "Method 1" },
                new LookupItem { Id = id2, Name = "Method 2" }
            };
            var isolationMethodsDto = new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = id1, Name = "Method 1" },
                new LookupItemDTO { Id = id2, Name = "Method 2" }
            };
            _mockLookupRepository.GetAllIsolationMethodsAsync().Returns(isolationMethods);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(isolationMethodsDto);

            // Act
            var result = await _mockLookupService.GetAllIsolationMethodsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, m => m.Name == "Method 1");
            Assert.Contains(result, m => m.Name == "Method 2");
        }

        [Fact]
        public async Task GetAllIsolationMethodsAsync_ShouldReturnEmptyList_WhenNoMethodsExist()
        {
            // Arrange            
            _mockLookupRepository.GetAllIsolationMethodsAsync().Returns(new List<LookupItem>());
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItemDTO>>()).Returns(new List<LookupItemDTO>());

            // Act
            var result = await _mockLookupService.GetAllIsolationMethodsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

       

        [Fact]
        public async Task GetAllIsolationMethodsAsync_ShouldCallRepositoryAndMapper()
        {
            // Arrange
            var isolationMethods = new List<LookupItem>();
            var isolationMethodsDto = new List<LookupItemDTO>();
            _mockLookupRepository.GetAllIsolationMethodsAsync().Returns(isolationMethods);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItemDTO>>()).Returns(isolationMethodsDto);

            // Act
            await _mockLookupService.GetAllIsolationMethodsAsync();

            // Assert
            await _mockLookupRepository.Received(1).GetAllIsolationMethodsAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>());
        }

    }
}
