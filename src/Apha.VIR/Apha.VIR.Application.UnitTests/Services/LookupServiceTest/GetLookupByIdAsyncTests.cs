using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.LookupServiceTest
{
    public class GetLookupByIdAsyncTests
    {
        private readonly ILookupRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _service;

        public GetLookupByIdAsyncTests()
        {
            _mockRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _service = new LookupService(_mockRepository, _mockMapper);
        }

        [Fact]
        public async Task GetLookupByIdAsync_ShouldReturnMappedDTO_WhenLookupExists()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var lookup = new Lookup { Id = lookupId, Name = "TestLookup" };
            var expectedDto = new LookupDTO { Id = lookupId, Name = "TestLookup" };

            _mockRepository.GetLookupByIdAsync(lookupId).Returns(lookup);
            _mockMapper.Map<LookupDTO>(lookup).Returns(expectedDto);

            // Act
            var result = await _service.GetLookupByIdAsync(lookupId);

            // Assert
            await _mockRepository.Received(1).GetLookupByIdAsync(lookupId);
            _mockMapper.Received(1).Map<LookupDTO>(lookup);
            Assert.Equal(expectedDto, result);
        }

        [Fact]
        public async Task GetLookupByIdAsync_ShouldReturnNull_WhenLookupDoesNotExist()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            _mockRepository.GetLookupByIdAsync(lookupId).Returns((Lookup)null);

            // Act
            var result = await _service.GetLookupByIdAsync(lookupId);

            // Assert
            await _mockRepository.Received(1).GetLookupByIdAsync(lookupId);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetLookupByIdAsync_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            _mockRepository.GetLookupByIdAsync(lookupId).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetLookupByIdAsync(lookupId));
        }
    }
}
