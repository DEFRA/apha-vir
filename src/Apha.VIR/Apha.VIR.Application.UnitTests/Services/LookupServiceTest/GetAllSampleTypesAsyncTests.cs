using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.LookupServiceTest
{
    public class GetAllSampleTypesAsyncTests
    {
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _mockLookupService;

        public GetAllSampleTypesAsyncTests()
        {
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockLookupService = new LookupService(_mockLookupRepository, _mockMapper);
        }

        [Fact]
        public async Task GetAllSampleTypesAsync_ReturnsExpectedSampleTypesList()
        {
            // Arrange
            var sampleTypes = new List<LookupItem>
            {
                new LookupItem { Id = Guid.NewGuid(), Name = "Sample Type 1" },
                new LookupItem { Id = Guid.NewGuid(), Name = "Sample Type 2" }
            };

            var expectedDtos = new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Sample Type 1" },
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Sample Type 2" }
            };

            _mockLookupRepository.GetAllSampleTypesAsync().Returns(sampleTypes);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(expectedDtos);

            // Act
            var result = await _mockLookupService.GetAllSampleTypesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<LookupItemDTO>>(result);
            Assert.Equal(expectedDtos, result);
            await _mockLookupRepository.Received(1).GetAllSampleTypesAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => x == sampleTypes));
        }

        [Fact]
        public async Task GetAllSampleTypesAsync_ThrowsException_WhenRepositoryFails()
        {
            // Arrange
            _mockLookupRepository.GetAllSampleTypesAsync().Throws(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllSampleTypesAsync());
        }

        [Fact]
        public async Task GetAllSampleTypesAsync_ReturnsEmptyList_WhenNoSampleTypesNotFound()
        {
            // Arrange
            _mockLookupRepository.GetAllSampleTypesAsync().Returns(new List<LookupItem>());
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(new List<LookupItemDTO>());

            // Act
            var result = await _mockLookupService.GetAllSampleTypesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<LookupItemDTO>>(result);
            Assert.Empty(result);
        }
    }
}
