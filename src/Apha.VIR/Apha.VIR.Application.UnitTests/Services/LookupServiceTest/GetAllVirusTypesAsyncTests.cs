using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;


namespace Apha.VIR.Application.UnitTests.Services.LookupServiceTest
{
    public class GetAllVirusTypesAsyncTests
    {
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _mockLookupService;

        public GetAllVirusTypesAsyncTests()
        {
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockLookupService = new LookupService(_mockLookupRepository, _mockMapper);
        }

        [Fact]
        public async Task GetAllVirusTypesAsync_ShouldReturnMappedVirusTypesList_WhenRepositoryReturnsData()
        {
            // Arrange
            var repositoryResult = new List<LookupItem>
            {
                new LookupItem { Id = Guid.NewGuid(), Name = "Virus Type 1" },
                new LookupItem { Id = Guid.NewGuid(), Name = "Virus Type 2" }
            };

            var expectedResult = new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Virus Type 1" },
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Virus Type 2" }
            };

            _mockLookupRepository.GetAllVirusTypesAsync().Returns(repositoryResult);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(expectedResult);

            // Act
            var result = await _mockLookupService.GetAllVirusTypesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult, result);
            await _mockLookupRepository.Received(1).GetAllVirusTypesAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>());
        }

        [Fact]
        public async Task GetAllVirusTypesAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange
            _mockLookupRepository.GetAllVirusTypesAsync().Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllVirusTypesAsync());
            await _mockLookupRepository.Received(1).GetAllVirusTypesAsync();
        }
    }
}
