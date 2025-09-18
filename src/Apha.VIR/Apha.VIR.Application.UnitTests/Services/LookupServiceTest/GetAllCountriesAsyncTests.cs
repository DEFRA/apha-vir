using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;

namespace Apha.VIR.Application.UnitTests.Services.LookupServiceTest
{
    public class GetAllCountriesAsyncTests
    {
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _mockLookupService;

        public GetAllCountriesAsyncTests()
        {
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockLookupService = new LookupService(_mockLookupRepository, _mockMapper);
        }

        [Fact]
        public async Task GetAllCountriesAsync_ShouldReturnCountries()
        {
            // Arrange            
            var countries = new List<LookupItem>
            {
                new LookupItem { Id = Guid.NewGuid(), Name = "Country 1" },
                new LookupItem { Id = Guid.NewGuid(), Name = "Country 2" }
            };

            var expectedDTOs = new List<LookupItemDto>
            {
                new LookupItemDto { Id = countries[0].Id, Name = countries[0].Name },
                new LookupItemDto { Id = countries[1].Id, Name = countries[1].Name }
            };

            _mockLookupRepository.GetAllCountriesAsync().Returns(countries);
            _mockMapper.Map<IEnumerable<LookupItemDto>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(expectedDTOs);

            // Act
            var result = await _mockLookupService.GetAllCountriesAsync();

            // Assert
            Assert.Equal(expectedDTOs, result);
            await _mockLookupRepository.Received(1).GetAllCountriesAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDto>>(Arg.Is<IEnumerable<LookupItem>>(x => x == countries));
        }

        [Fact]
        public async Task GetAllCountriesAsync_ShouldReturnEmptyList_WhenNoCountriesExist()
        {
            // Arrange            
            _mockLookupRepository.GetAllCountriesAsync().Returns(new List<LookupItem>());
            _mockMapper.Map<IEnumerable<LookupItemDto>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(new List<LookupItemDto>());

            // Act
            var result = await _mockLookupService.GetAllCountriesAsync();

            // Assert
            Assert.Empty(result);
            await _mockLookupRepository.Received(1).GetAllCountriesAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDto>>(Arg.Is<IEnumerable<LookupItem>>(x => !x.Any()));
        }

        [Fact]
        public async Task GetAllCountriesAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange          
            _mockLookupRepository.GetAllCountriesAsync().Returns<Task<IEnumerable<LookupItem>>>(_ => throw new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllCountriesAsync());
            await _mockLookupRepository.Received(1).GetAllCountriesAsync();
            _mockMapper.DidNotReceive().Map<IEnumerable<LookupItemDto>>(Arg.Any<IEnumerable<LookupItem>>());
        }
    }
}
