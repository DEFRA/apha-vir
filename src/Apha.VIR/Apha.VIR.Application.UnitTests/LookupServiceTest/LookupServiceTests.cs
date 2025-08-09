using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.LookupServiceTest
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
            _mockLookupRepository.GetAllLookupsAsync().Returns((IEnumerable<Lookup>)null);
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
            string? virusFamily = null;
            var repositoryResult = new List<LookupItemByParent>();
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
            var virusFamily = string.Empty;
            var repositoryResult = new List<LookupItemByParent>();
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
            var virusFamily = "TestFamily";
            var emptyList = new List<LookupItemByParent>();
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
            var virusFamily = "TestFamily";
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
            var hostSpecies = "Dog";
            var lookupItems = new List<LookupItemByParent> { new LookupItemByParent { Id = Guid.NewGuid(), Name = "Labrador" } };
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
            string? hostSpecies = null;
            var lookupItems = new List<LookupItemByParent> { new LookupItemByParent { Id = Guid.NewGuid(), Name = "All Breeds" } };
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
            var hostSpecies = "Cat";
            var emptyList = new List<LookupItemByParent>();
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
            var hostSpecies = "Bird";
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
    }
}
