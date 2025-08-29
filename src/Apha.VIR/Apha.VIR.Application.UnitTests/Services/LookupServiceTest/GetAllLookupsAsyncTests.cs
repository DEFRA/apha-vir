using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.LookupServiceTest
{
    public class GetAllLookupsAsyncTests
    {
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _mockLookupService;

        public GetAllLookupsAsyncTests()
        {
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockLookupService = new LookupService(_mockLookupRepository, _mockMapper);
        }

        [Fact]
        public async Task GetAllLookupsAsync_ShouldReturnMappedDTOsList_WhenRepositoryReturnsData()
        {
            // Arrange
            var lookups = new List<Lookup> { new Lookup(), new Lookup() };
            var expectedDTOs = new List<LookupDTO> { new LookupDTO(), new LookupDTO() };

            _mockLookupRepository.GetAllLookupsAsync().Returns(lookups);
            _mockMapper.Map<IEnumerable<LookupDTO>>(Arg.Any<IEnumerable<Lookup>>()).Returns(expectedDTOs);

            // Act
            var result = await _mockLookupService.GetAllLookupsAsync();

            // Assert
            Assert.Equal(expectedDTOs, result);
            await _mockLookupRepository.Received(1).GetAllLookupsAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupDTO>>(Arg.Is<IEnumerable<Lookup>>(l => l == lookups));
        }

        [Fact]
        public async Task GetAllLookupsAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange
            var expectedException = new Exception("Repository error");
            _mockLookupRepository.GetAllLookupsAsync().Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllLookupsAsync());
            Assert.Equal(expectedException.Message, exception.Message);
        }

        [Fact]
        public async Task GetAllLookupsAsync_ShouldReturnCorrectNumberOfDTOs_WhenRepositoryReturnsData()
        {
            // Arrange
            var lookups = new List<Lookup> { new Lookup(), new Lookup(), new Lookup() };
            var expectedDTOs = new List<LookupDTO> { new LookupDTO(), new LookupDTO(), new LookupDTO() };

            _mockLookupRepository.GetAllLookupsAsync().Returns(lookups);
            _mockMapper.Map<IEnumerable<LookupDTO>>(Arg.Any<IEnumerable<Lookup>>()).Returns(expectedDTOs);

            // Act
            var result = await _mockLookupService.GetAllLookupsAsync();

            // Assert
            Assert.Equal(3, result.Count());
            await _mockLookupRepository.Received(1).GetAllLookupsAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupDTO>>(Arg.Is<IEnumerable<Lookup>>(l => l.Count() == 3));
        }
    }
}
