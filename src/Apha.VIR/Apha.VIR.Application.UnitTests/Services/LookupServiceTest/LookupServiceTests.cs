using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.LookupServiceTest
{
    public class LookupServiceTests
    {
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _mockLookupService;

        public LookupServiceTests()
        {
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockLookupService = new LookupService(_mockLookupRepository, _mockMapper);
        }

        [Fact]
        public async Task GetAllLookupsAsync_ShouldReturnMappedDTOs_WhenRepositoryReturnsData()
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
        public async Task GetAllLookupsAsync_ShouldReturnEmptyList_WhenRepositoryReturnsNull()
        {
            // Arrange
            _mockLookupRepository.GetAllLookupsAsync().Returns((IEnumerable<Lookup>)null!);
            _mockMapper.Map<IEnumerable<LookupDTO>>(Arg.Any<IEnumerable<Lookup>>()).Returns(new List<LookupDTO>());

            // Act
            var result = await _mockLookupService.GetAllLookupsAsync();

            // Assert
            Assert.Empty(result);
            await _mockLookupRepository.Received(1).GetAllLookupsAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupDTO>>(Arg.Any<IEnumerable<Lookup>>());
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

        [Fact]
        public async Task GetAllLookupEntriesAsync_SuccessfulRetrieval_ReturnsLookupItems()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var lookupItems = new List<LookupItem>
            {
                new LookupItem { Id = Guid.NewGuid(), Name = "Item1" },
                new LookupItem { Id = Guid.NewGuid(), Name = "Item2" }
            };
            var expectedDtos = new List<LookupItemDTO>
            {
            new LookupItemDTO { Id = lookupItems[0].Id, Name = lookupItems[0].Name },
            new LookupItemDTO { Id = lookupItems[1].Id, Name = lookupItems[1].Name }
            };

            _mockLookupRepository.GetAllLookupEntriesAsync(lookupId).Returns(lookupItems);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(expectedDtos);

            // Act
            var result = await _mockLookupService.GetAllLookupEntriesAsync(lookupId);

            // Assert
            await _mockLookupRepository.Received(1).GetAllLookupEntriesAsync(lookupId);
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>());
            Assert.Equal(expectedDtos, result);
        }

        [Fact]
        public async Task GetAllLookupEntriesAsync_ExceptionThrown_PropagatesException()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var expectedException = new Exception("Test exception");

            _mockLookupRepository.GetAllLookupEntriesAsync(lookupId).Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllLookupEntriesAsync(lookupId));
            Assert.Same(expectedException, exception);
            await _mockLookupRepository.Received(1).GetAllLookupEntriesAsync(lookupId);
            _mockMapper.DidNotReceive().Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>());
        }

        [Fact]
        public async Task GetAllVirusFamiliesAsync_ShouldReturnMappedVirusFamilies_WhenRepositoryReturnsData()
        {
            // Arrange
            var virusFamilies = new List<LookupItem>
            {
                new LookupItem { Id = Guid.NewGuid(), Name = "Family1" },
                new LookupItem { Id = Guid.NewGuid(), Name = "Family2" }
            };
            var expectedDtos = new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Family1" },
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Family2" }
            };

            _mockLookupRepository.GetAllVirusFamiliesAsync().Returns(virusFamilies);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(expectedDtos);

            // Act
            var result = await _mockLookupService.GetAllVirusFamiliesAsync();

            // Assert
            Assert.Equal(expectedDtos, result);
            await _mockLookupRepository.Received(1).GetAllVirusFamiliesAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => x == virusFamilies));
        }

        [Fact]
        public async Task GetAllVirusFamiliesAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange
            _mockLookupRepository.GetAllVirusFamiliesAsync().Throws(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllVirusFamiliesAsync());
        }

        [Fact]
        public async Task GetAllVirusFamiliesAsync_ShouldReturnEmptyList_WhenRepositoryReturnsEmptyList()
        {
            // Arrange
            var emptyList = new List<LookupItem>();
            var emptyDtoList = new List<LookupItemDTO>();

            _mockLookupRepository.GetAllVirusFamiliesAsync().Returns(emptyList);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(emptyDtoList);

            // Act
            var result = await _mockLookupService.GetAllVirusFamiliesAsync();

            // Assert
            Assert.Empty(result);
            await _mockLookupRepository.Received(1).GetAllVirusFamiliesAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => x == emptyList));
        }

        [Fact]
        public async Task GetAllVirusFamiliesAsync_ShouldReturnCorrectType()
        {
            // Arrange
            _mockLookupRepository.GetAllVirusFamiliesAsync().Returns(new List<LookupItem>());
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(new List<LookupItemDTO>());

            // Act
            var result = await _mockLookupService.GetAllVirusFamiliesAsync();

            // Assert
            Assert.IsAssignableFrom<IEnumerable<LookupItemDTO>>(result);
        }

        [Fact]
        public async Task GetAllVirusTypesAsync_ShouldReturnMappedVirusTypes_WhenRepositoryReturnsData()
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

        [Fact]
        public async Task GetAllVirusTypesByParentAsync_WithNullVirusFamily_ReturnsExpectedResult()
        {
            // Arrange
            Guid? virusFamily = null;
            var repositoryResult = new List<LookupItem>();
            var expectedResult = new List<LookupItemDTO>();

            _mockLookupRepository.GetAllVirusTypesByParentAsync(virusFamily).Returns(repositoryResult);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(expectedResult);

            // Act
            var result = await _mockLookupService.GetAllVirusTypesByParentAsync(virusFamily);

            // Assert
            await _mockLookupRepository.Received(1).GetAllVirusTypesByParentAsync(virusFamily);
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(repositoryResult);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetAllVirusTypesByParentAsync_WithEmptyVirusFamily_ReturnsExpectedResult()
        {
            // Arrange
            Guid? virusFamily = null;
            var repositoryResult = new List<LookupItem>();
            var expectedResult = new List<LookupItemDTO>();

            _mockLookupRepository.GetAllVirusTypesByParentAsync(virusFamily).Returns(repositoryResult);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(expectedResult);

            // Act
            var result = await _mockLookupService.GetAllVirusTypesByParentAsync(virusFamily);

            // Assert
            await _mockLookupRepository.Received(1).GetAllVirusTypesByParentAsync(virusFamily);
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(repositoryResult);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetAllVirusTypesByParentAsync_WhenRepositoryReturnsEmptyList_ReturnsEmptyList()
        {
            // Arrange
            Guid virusFamily = Guid.NewGuid();
            var emptyList = new List<LookupItem>();
            var emptyDTOList = new List<LookupItemDTO>();

            _mockLookupRepository.GetAllVirusTypesByParentAsync(virusFamily).Returns(emptyList);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(emptyList).Returns(emptyDTOList);

            // Act
            var result = await _mockLookupService.GetAllVirusTypesByParentAsync(virusFamily);

            // Assert
            await _mockLookupRepository.Received(1).GetAllVirusTypesByParentAsync(virusFamily);
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(emptyList);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllVirusTypesByParentAsync_WhenRepositoryThrowsException_ThrowsException()
        {
            // Arrange
            Guid virusFamily = Guid.NewGuid();
            var expectedException = new Exception("Test exception");

            _mockLookupRepository.GetAllVirusTypesByParentAsync(virusFamily).Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllVirusTypesByParentAsync(virusFamily));
            Assert.Equal(expectedException.Message, exception.Message);
            await _mockLookupRepository.Received(1).GetAllVirusTypesByParentAsync(virusFamily);
            _mockMapper.DidNotReceive().Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>());
        }

        [Fact]
        public async Task GetAllHostSpeciesAsync_ShouldReturnExpectedResult()
        {
            // Arrange
            var hostSpecies = new List<LookupItem>
            {
                new LookupItem { Id = Guid.NewGuid(), Name = "Species 1" },
                new LookupItem { Id = Guid.NewGuid(), Name = "Species 2" }
            };
            var expectedDTOs = new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = hostSpecies[0].Id, Name = hostSpecies[0].Name },
                new LookupItemDTO { Id = hostSpecies[1].Id, Name = hostSpecies[1].Name }
            };

            _mockLookupRepository.GetAllHostSpeciesAsync().Returns(hostSpecies);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(expectedDTOs);

            // Act
            var result = await _mockLookupService.GetAllHostSpeciesAsync();

            // Assert
            Assert.Equal(expectedDTOs, result);
            await _mockLookupRepository.Received(1).GetAllHostSpeciesAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => x == hostSpecies));
        }

        [Fact]
        public async Task GetAllHostSpeciesAsync_ShouldCallRepositoryOnce()
        {
            // Arrange
            _mockLookupRepository.GetAllHostSpeciesAsync().Returns(new List<LookupItem>());

            // Act
            await _mockLookupService.GetAllHostSpeciesAsync();

            // Assert
            await _mockLookupRepository.Received(1).GetAllHostSpeciesAsync();
        }

        [Fact]
        public async Task GetAllHostSpeciesAsync_ShouldCallMapperWithCorrectParameters()
        {
            // Arrange
            var hostSpecies = new List<LookupItem>();
            _mockLookupRepository.GetAllHostSpeciesAsync().Returns(hostSpecies);

            // Act
            await _mockLookupService.GetAllHostSpeciesAsync();

            // Assert
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => x == hostSpecies));
        }

        [Fact]
        public async Task GetAllHostSpeciesAsync_ShouldReturnEmptyList_WhenRepositoryReturnsEmptyList()
        {
            // Arrange
            _mockLookupRepository.GetAllHostSpeciesAsync().Returns(new List<LookupItem>());
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(new List<LookupItemDTO>());

            // Act
            var result = await _mockLookupService.GetAllHostSpeciesAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllHostSpeciesAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange
            var expectedException = new Exception("Test exception");
            _mockLookupRepository.GetAllHostSpeciesAsync().Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllHostSpeciesAsync());
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public async Task GetAllHostBreedsAsync_ReturnsExpectedResult()
        {
            // Arrange
            var mockHostBreeds = new List<LookupItem>
            {
                new LookupItem { Id = Guid.NewGuid(), Name = "Breed1" },
                new LookupItem { Id = Guid.NewGuid(), Name = "Breed2" }
            };

            var expectedDTOs = new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = mockHostBreeds[0].Id, Name = mockHostBreeds[0].Name },
                new LookupItemDTO { Id = mockHostBreeds[1].Id, Name = mockHostBreeds[1].Name }
            };

            _mockLookupRepository.GetAllHostBreedsAsync().Returns(mockHostBreeds);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(expectedDTOs);

            // Act
            var result = await _mockLookupService.GetAllHostBreedsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDTOs, result);
            await _mockLookupRepository.Received(1).GetAllHostBreedsAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => x == mockHostBreeds));
        }

        [Fact]
        public async Task GetAllHostBreedsAsync_ThrowsException_WhenRepositoryFails()
        {
            // Arrange
            _mockLookupRepository.GetAllHostBreedsAsync().Throws(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllHostBreedsAsync());
            await _mockLookupRepository.Received(1).GetAllHostBreedsAsync();
        }

        [Fact]
        public async Task GetAllHostBreedsByParentAsync_ValidHostSpecies_ReturnsLookupItemDTOs()
        {
            // Arrange
            Guid hostSpecies = Guid.NewGuid();
            var lookupItems = new List<LookupItem> { new LookupItem { Id = Guid.NewGuid(), Name = "Labrador" } };
            var lookupItemDTOs = new List<LookupItemDTO> { new LookupItemDTO { Id = Guid.NewGuid(), Name = "Labrador" } };

            _mockLookupRepository.GetAllHostBreedsByParentAsync(hostSpecies).Returns(lookupItems);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(lookupItems).Returns(lookupItemDTOs);

            // Act
            var result = await _mockLookupService.GetAllHostBreedsByParentAsync(hostSpecies);

            // Assert
            await _mockLookupRepository.Received(1).GetAllHostBreedsByParentAsync(hostSpecies);
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(lookupItems);
            Assert.Equal(lookupItemDTOs, result);
        }

        [Fact]
        public async Task GetAllHostBreedsByParentAsync_NullHostSpecies_ReturnsLookupItemDTOs()
        {
            // Arrange
            Guid? hostSpecies = null;
            var lookupItems = new List<LookupItem> { new LookupItem { Id = Guid.NewGuid(), Name = "All Breeds" } };
            var lookupItemDTOs = new List<LookupItemDTO> { new LookupItemDTO { Id = Guid.NewGuid(), Name = "All Breeds" } };

            _mockLookupRepository.GetAllHostBreedsByParentAsync(hostSpecies).Returns(lookupItems);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(lookupItems).Returns(lookupItemDTOs);

            // Act
            var result = await _mockLookupService.GetAllHostBreedsByParentAsync(hostSpecies);

            // Assert
            await _mockLookupRepository.Received(1).GetAllHostBreedsByParentAsync(hostSpecies);
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(lookupItems);
            Assert.Equal(lookupItemDTOs, result);
        }

        [Fact]
        public async Task GetAllHostBreedsByParentAsync_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            Guid hostSpecies = Guid.NewGuid();
            var emptyList = new List<LookupItem>();
            var emptyDTOList = new List<LookupItemDTO>();

            _mockLookupRepository.GetAllHostBreedsByParentAsync(hostSpecies).Returns(emptyList);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(emptyList).Returns(emptyDTOList);

            // Act
            var result = await _mockLookupService.GetAllHostBreedsByParentAsync(hostSpecies);

            // Assert
            await _mockLookupRepository.Received(1).GetAllHostBreedsByParentAsync(hostSpecies);
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(emptyList);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllHostBreedsByParentAsync_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            Guid hostSpecies = Guid.NewGuid();
            var expectedException = new Exception("Repository error");

            _mockLookupRepository.GetAllHostBreedsByParentAsync(hostSpecies).Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllHostBreedsByParentAsync(hostSpecies));
            Assert.Equal(expectedException.Message, exception.Message);
            await _mockLookupRepository.Received(1).GetAllHostBreedsByParentAsync(hostSpecies);
        }

        [Fact]
        public async Task GetAllCountriesAsync_ShouldReturnMappedCountries()
        {
            // Arrange            
            var countries = new List<LookupItem>
            {
                new LookupItem { Id = Guid.NewGuid(), Name = "Country 1" },
                new LookupItem { Id = Guid.NewGuid(), Name = "Country 2" }
            };

            var expectedDTOs = new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = countries[0].Id, Name = countries[0].Name },
                new LookupItemDTO { Id = countries[1].Id, Name = countries[1].Name }
            };

            _mockLookupRepository.GetAllCountriesAsync().Returns(countries);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(expectedDTOs);

            // Act
            var result = await _mockLookupService.GetAllCountriesAsync();

            // Assert
            Assert.Equal(expectedDTOs, result);
            await _mockLookupRepository.Received(1).GetAllCountriesAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => x == countries));
        }

        [Fact]
        public async Task GetAllCountriesAsync_ShouldReturnEmptyList_WhenNoCountriesExist()
        {
            // Arrange            
            _mockLookupRepository.GetAllCountriesAsync().Returns(new List<LookupItem>());
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(new List<LookupItemDTO>());

            // Act
            var result = await _mockLookupService.GetAllCountriesAsync();

            // Assert
            Assert.Empty(result);
            await _mockLookupRepository.Received(1).GetAllCountriesAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => !x.Any()));
        }

        [Fact]
        public async Task GetAllCountriesAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange          
            _mockLookupRepository.GetAllCountriesAsync().Returns<Task<IEnumerable<LookupItem>>>(_ => throw new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllCountriesAsync());
            await _mockLookupRepository.Received(1).GetAllCountriesAsync();
            _mockMapper.DidNotReceive().Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>());
        }

        [Fact]
        public async Task GetAllHostPurposesAsync_SuccessfulScenario_ReturnsExpectedResult()
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
        public async Task GetAllHostPurposesAsync_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            _mockLookupRepository.GetAllHostPurposesAsync().Throws(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllHostPurposesAsync());
            await _mockLookupRepository.Received(1).GetAllHostPurposesAsync();
        }

        [Fact]
        public async Task GetAllHostPurposesAsync_MapperThrowsException_ThrowsException()
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

        [Fact]
        public async Task GetAllSampleTypesAsync_ReturnsExpectedResult()
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
        public async Task GetAllSampleTypesAsync_ReturnsEmptyList_WhenNoSampleTypesFound()
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

        [Fact]
        public async Task GetAllWorkGroupsAsync_ReturnsCorrectData()
        {
            // Arrange
            var workGroups = new List<LookupItem>
            {
            new LookupItem { Id = Guid.NewGuid(), Name = "Group 1" },
            new LookupItem { Id = Guid.NewGuid(), Name = "Group 2" }
            };
            var workGroupDTOs = new List<LookupItemDTO>
            {
            new LookupItemDTO { Id = workGroups[0].Id, Name = workGroups[0].Name },
            new LookupItemDTO { Id = workGroups[1].Id, Name = workGroups[1].Name }
            };

            _mockLookupRepository.GetAllWorkGroupsAsync().Returns(workGroups);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(workGroupDTOs);

            // Act
            var result = await _mockLookupService.GetAllWorkGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            await _mockLookupRepository.Received(1).GetAllWorkGroupsAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => x == workGroups));
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_ReturnsEmptyList_WhenRepositoryReturnsEmptyList()
        {
            // Arrange
            _mockLookupRepository.GetAllWorkGroupsAsync().Returns(new List<LookupItem>());
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(new List<LookupItemDTO>());

            // Act
            var result = await _mockLookupService.GetAllWorkGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_ThrowsException_WhenRepositoryThrowsException()
        {
            // Arrange
            _mockLookupRepository.GetAllWorkGroupsAsync().ThrowsAsync(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllWorkGroupsAsync());
        }

        [Fact]
        public async Task Test_GetAllStaffAsync_ReturnsStaffData()
        {
            // Arrange
            var staffEntities = new List<LookupItem>
            {
            new LookupItem { Id = Guid.NewGuid(), Name = "Staff 1" },
            new LookupItem { Id = Guid.NewGuid(), Name = "Staff 2" }
            };
            var staffDtos = new List<LookupItemDTO>
            {
            new LookupItemDTO { Id = Guid.NewGuid(), Name = "Staff 1" },
            new LookupItemDTO { Id = Guid.NewGuid(), Name = "Staff 2" }
            };

            _mockLookupRepository.GetAllStaffAsync().Returns(staffEntities);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(staffDtos);

            // Act
            var result = await _mockLookupService.GetAllStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            await _mockLookupRepository.Received(1).GetAllStaffAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => x == staffEntities));
        }

        [Fact]
        public async Task Test_GetAllStaffAsync_ReturnsEmptyList()
        {
            // Arrange
            var emptyList = new List<LookupItem>();
            _mockLookupRepository.GetAllStaffAsync().Returns(emptyList);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(new List<LookupItemDTO>());

            // Act
            var result = await _mockLookupService.GetAllStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            await _mockLookupRepository.Received(1).GetAllStaffAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => x == emptyList));
        }

        [Fact]
        public async Task Test_GetAllStaffAsync_ThrowsException()
        {
            // Arrange
            _mockLookupRepository.GetAllStaffAsync().Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllStaffAsync());
            await _mockLookupRepository.Received(1).GetAllStaffAsync();
            _mockMapper.DidNotReceive().Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>());
        }

        [Fact]
        public async Task Test_GetAllViabilityAsync_ReturnsExpectedResult()
        {
            // Arrange
            var viabilityEntities = new List<LookupItem> { new LookupItem(), new LookupItem() };
            var expectedDtos = new List<LookupItemDTO> { new LookupItemDTO(), new LookupItemDTO() };

            _mockLookupRepository.GetAllViabilityAsync().Returns(viabilityEntities);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(viabilityEntities).Returns(expectedDtos);

            // Act
            var result = await _mockLookupService.GetAllViabilityAsync();

            // Assert
            Assert.Equal(expectedDtos, result);
        }

        [Fact]
        public async Task Test_GetAllViabilityAsync_CallsRepositoryMethod()
        {
            // Arrange
            _mockLookupRepository.GetAllViabilityAsync().Returns(new List<LookupItem>());

            // Act
            await _mockLookupService.GetAllViabilityAsync();

            // Assert
            await _mockLookupRepository.Received(1).GetAllViabilityAsync();
        }

        [Fact]
        public async Task Test_GetAllViabilityAsync_HandlesEmptyResult()
        {
            // Arrange
            _mockLookupRepository.GetAllViabilityAsync().Returns(new List<LookupItem>());
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(new List<LookupItemDTO>());

            // Act
            var result = await _mockLookupService.GetAllViabilityAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllSubmittingLabAsync_ShouldReturnMappedResults()
        {
            // Arrange
            var submittingLabs = new List<LookupItem>
            {
            new LookupItem { Id = Guid.NewGuid(), Name = "Lab 1" },
            new LookupItem { Id = Guid.NewGuid(), Name = "Lab 2" }
            };
            var expectedDtos = new List<LookupItemDTO>
            {
            new LookupItemDTO { Id = submittingLabs[0].Id, Name = submittingLabs[0].Name },
            new LookupItemDTO { Id = submittingLabs[1].Id, Name = submittingLabs[1].Name }
            };

            _mockLookupRepository.GetAllSubmittingLabAsync().Returns(submittingLabs);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(expectedDtos);

            // Act
            var result = await _mockLookupService.GetAllSubmittingLabAsync();

            // Assert
            Assert.Equal(expectedDtos, result);
            await _mockLookupRepository.Received(1).GetAllSubmittingLabAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => x == submittingLabs));
        }

        [Fact]
        public async Task GetAllSubmittingLabAsync_ShouldReturnEmptyList_WhenNoLabsExist()
        {
            // Arrange
            _mockLookupRepository.GetAllSubmittingLabAsync().Returns(new List<LookupItem>());
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(new List<LookupItemDTO>());

            // Act
            var result = await _mockLookupService.GetAllSubmittingLabAsync();

            // Assert
            Assert.Empty(result);
            await _mockLookupRepository.Received(1).GetAllSubmittingLabAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => !x.Any()));
        }

        [Fact]
        public async Task GetAllSubmittingLabAsync_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            _mockLookupRepository.GetAllSubmittingLabAsync().Returns(Task.FromException<IEnumerable<LookupItem>>(new Exception("Repository error")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllSubmittingLabAsync());
            await _mockLookupRepository.Received(1).GetAllSubmittingLabAsync();
        }

        [Fact]
        public async Task GetAllSubmittingLabAsync_ShouldThrowException_WhenMapperFails()
        {
            // Arrange
            _mockLookupRepository.GetAllSubmittingLabAsync().Returns(new List<LookupItem>());
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Throws(new AutoMapperMappingException("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _mockLookupService.GetAllSubmittingLabAsync());
            await _mockLookupRepository.Received(1).GetAllSubmittingLabAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>());
        }

        [Fact]
        public async Task GetAllSubmissionReasonAsync_ShouldReturnMappedDTOs_WhenReasonExist()
        {
            // Arrange
            var mockReasons = new List<LookupItem> { new LookupItem(), new LookupItem() };
            var expectedDtos = new List<LookupItemDTO> { new LookupItemDTO(), new LookupItemDTO() };

            _mockLookupRepository.GetAllSubmissionReasonAsync().Returns(mockReasons);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(expectedDtos);

            // Act
            var result = await _mockLookupService.GetAllSubmissionReasonAsync();

            // Assert
            Assert.Equal(expectedDtos, result);
            await _mockLookupRepository.Received(1).GetAllSubmissionReasonAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => x == mockReasons));
        }

        [Fact]
        public async Task GetAllSubmissionReasonAsync_ShouldReturnEmptyList_WhenNoReasonsExist()
        {
            // Arrange
            var emptyList = new List<LookupItem>();
            _mockLookupRepository.GetAllSubmissionReasonAsync().Returns(emptyList);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(new List<LookupItemDTO>());

            // Act
            var result = await _mockLookupService.GetAllSubmissionReasonAsync();

            // Assert
            Assert.Empty(result);
            await _mockLookupRepository.Received(1).GetAllSubmissionReasonAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => x == emptyList));
        }

        [Fact]
        public async Task GetAllSubmissionReasonAsync_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            _mockLookupRepository.GetAllSubmissionReasonAsync().Returns(Task.FromException<IEnumerable<LookupItem>>(new Exception("Repository error")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllSubmissionReasonAsync());
            await _mockLookupRepository.Received(1).GetAllSubmissionReasonAsync();
        }
    }
}
