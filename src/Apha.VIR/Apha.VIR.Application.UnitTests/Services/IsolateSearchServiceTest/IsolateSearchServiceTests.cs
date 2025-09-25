using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Pagination;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.Core.Pagination;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.IsolateSearchServiceTest
{
    public class IsolateSearchServiceTests
    {
        private readonly IVirusCharacteristicRepository _mockVirusCharacteristicRepository;
        private readonly IVirusCharacteristicListEntryRepository _mockVirusCharacteristicListEntryRepository;
        private readonly IIsolateSearchRepository _mockIsolateSearchRepository;
        private readonly IIsolateRepository _mockIsolateRepository;
        private readonly IMapper _mockMapper;
        private readonly IsolateSearchService _mockIsolateSearchService;

        public IsolateSearchServiceTests()
        {
            _mockVirusCharacteristicRepository = Substitute.For<IVirusCharacteristicRepository>();
            _mockVirusCharacteristicListEntryRepository = Substitute.For<IVirusCharacteristicListEntryRepository>();
            _mockIsolateSearchRepository = Substitute.For<IIsolateSearchRepository>();
            _mockIsolateRepository = Substitute.For<IIsolateRepository>();
            _mockMapper = Substitute.For<IMapper>();

            _mockIsolateSearchService = new IsolateSearchService(
            _mockVirusCharacteristicRepository,
            _mockVirusCharacteristicListEntryRepository,
            _mockIsolateSearchRepository,
            _mockIsolateRepository,
            _mockMapper
            );
        }

        [Fact]
        public async Task GetComparatorsAndListValuesAsync_NumericCharacteristic_ReturnsCorrectComparators()
        {
            // Arrange
            var characteristicId = Guid.NewGuid();
            SetupMockedCharacteristics(new VirusCharacteristicDto { Id = characteristicId, DataType = "Numeric" });

            // Act
            var result = await _mockIsolateSearchService.GetComparatorsAndListValuesAsync(characteristicId);

            // Assert
            Assert.Equal(new List<string> { "=", "<", ">", "<=", ">=", "between" }, result.Item1);
            Assert.Empty(result.Item2);
        }

        [Fact]
        public async Task GetComparatorsAndListValuesAsync_SingleListCharacteristic_ReturnsCorrectComparatorsAndListValues()
        {
            // Arrange
            var characteristicId = Guid.NewGuid();
            SetupMockedCharacteristics(new VirusCharacteristicDto { Id = characteristicId, DataType = "SingleList" });
            var listEntries = new List<VirusCharacteristicListEntry> { new VirusCharacteristicListEntry() };
            _mockVirusCharacteristicListEntryRepository.GetEntriesByCharacteristicIdAsync(characteristicId)
            .Returns(listEntries);
            _mockMapper.Map<IEnumerable<VirusCharacteristicListEntryDto>>(Arg.Any<IEnumerable<VirusCharacteristicListEntry>>())
            .Returns(listEntries.Select(e => new VirusCharacteristicListEntryDto()));

            // Act
            var result = await _mockIsolateSearchService.GetComparatorsAndListValuesAsync(characteristicId);

            // Assert
            Assert.Equal(new List<string> { "=", "not equal to", "begins with" }, result.Item1);
            Assert.Single(result.Item2);
        }

        [Fact]
        public async Task GetComparatorsAndListValuesAsync_YesNoCharacteristic_ReturnsCorrectComparators()
        {
            // Arrange
            var characteristicId = Guid.NewGuid();
            SetupMockedCharacteristics(new VirusCharacteristicDto { Id = characteristicId, DataType = "Yes/No" });

            // Act
            var result = await _mockIsolateSearchService.GetComparatorsAndListValuesAsync(characteristicId);

            // Assert
            Assert.Equal(new List<string> { "=" }, result.Item1);
            Assert.NotEmpty(result.Item2);
        }

        [Fact]
        public async Task GetComparatorsAndListValuesAsync_TextCharacteristic_ReturnsCorrectComparators()
        {
            // Arrange
            var characteristicId = Guid.NewGuid();
            SetupMockedCharacteristics(new VirusCharacteristicDto { Id = characteristicId, DataType = "Text" });

            // Act
            var result = await _mockIsolateSearchService.GetComparatorsAndListValuesAsync(characteristicId);

            // Assert
            Assert.Equal(new List<string> { "=", "contains" }, result.Item1);
            Assert.Empty(result.Item2);
        }

        [Fact]
        public async Task GetComparatorsAndListValuesAsync_InvalidGuid_ReturnsEmptyLists()
        {
            // Arrange
            var characteristicId = Guid.NewGuid();
            SetupMockedCharacteristics();

            // Act
            var result = await _mockIsolateSearchService.GetComparatorsAndListValuesAsync(characteristicId);

            // Assert
            Assert.Empty(result.Item1);
            Assert.Empty(result.Item2);
        }

        private void SetupMockedCharacteristics(params VirusCharacteristicDto[] characteristics)
        {
            _mockVirusCharacteristicRepository.GetAllVirusCharacteristicsAsync()
            .Returns(characteristics.Select(c => new VirusCharacteristic()));
            _mockMapper.Map<IEnumerable<VirusCharacteristicDto>>(Arg.Any<IEnumerable<VirusCharacteristic>>())
            .Returns(characteristics);
        }

        [Fact]
        public async Task PerformSearchAsync_WithValidInput_ReturnsSuccessfulSearch()
        {
            // Arrange
            var queryParams = new QueryParameters<SearchCriteriaDTO>
            {
                Filter = new SearchCriteriaDTO(),
                Page = 1,
                PageSize = 10
            };

            var mappedParams = new PaginationParameters<SearchCriteria>
            {
                Filter = new SearchCriteria(),
                Page = 1,
                PageSize = 10
            };

            var isolateSearchResult = new List<IsolateSearchResult> { new IsolateSearchResult() };
            var searchResult = new PagedData<IsolateSearchResult>(isolateSearchResult, totalCount: 1);

            var expectedResult = new PaginatedResult<IsolateSearchResultDto>
            {
                data = new List<IsolateSearchResultDto> { new IsolateSearchResultDto() },
                TotalCount = 1
            };

            _mockMapper.Map<PaginationParameters<SearchCriteria>>(queryParams).Returns(mappedParams);
            _mockIsolateSearchRepository.PerformSearchAsync(Arg.Any<PaginationParameters<SearchCriteria>>()).Returns(searchResult);
            _mockMapper.Map<PaginatedResult<IsolateSearchResultDto>>(searchResult).Returns(expectedResult);

            // Act
            var result = await _mockIsolateSearchService.PerformSearchAsync(queryParams);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.TotalCount, result.TotalCount);
            Assert.Equal(expectedResult.data.Count(), result.data.Count());
        }

        [Fact]
        public async Task PerformSearchAsync_WithEmptyCriteria_ReturnsEmptyResult()
        {
            // Arrange
            var queryParams = new QueryParameters<SearchCriteriaDTO>
            {
                Filter = null,
                Page = 1,
                PageSize = 10
            };

            var mappedParams = new PaginationParameters<SearchCriteria>
            {
                Filter = null,
                Page = 1,
                PageSize = 10
            };

            var isolateSearchResult = new List<IsolateSearchResult>();
            var searchResult = new PagedData<IsolateSearchResult>(isolateSearchResult, totalCount: 0);

            var expectedResult = new PaginatedResult<IsolateSearchResultDto>
            {
                data = new List<IsolateSearchResultDto>(),
                TotalCount = 0
            };

            _mockMapper.Map<PaginationParameters<SearchCriteria>>(queryParams).Returns(mappedParams);
            _mockIsolateSearchRepository.PerformSearchAsync(Arg.Any<PaginationParameters<SearchCriteria>>()).Returns(searchResult);
            _mockMapper.Map<PaginatedResult<IsolateSearchResultDto>>(searchResult).Returns(expectedResult);

            // Act
            var result = await _mockIsolateSearchService.PerformSearchAsync(queryParams);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.data);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task PerformSearchAsync_WithInvalidInput_ThrowsException()
        {
            // Arrange
            var queryParams = new QueryParameters<SearchCriteriaDTO>
            {
                Filter = new SearchCriteriaDTO(),
                Page = -1, // Invalid page number
                PageSize = 10
            };

            _mockMapper.Map<PaginationParameters<SearchCriteria>>(queryParams).Returns(x => { throw new ArgumentException("Invalid page number"); });

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _mockIsolateSearchService.PerformSearchAsync(queryParams));
        }

        [Fact]
        public async Task PerformSearchAsync_ErrorHandling_ThrowsException()
        {
            // Arrange
            var queryParams = new QueryParameters<SearchCriteriaDTO>
            {
                Filter = new SearchCriteriaDTO(),
                Page = 1,
                PageSize = 10
            };

            var mappedParams = new PaginationParameters<SearchCriteria>
            {
                Filter = new SearchCriteria(),
                Page = 1,
                PageSize = 10
            };

            _mockMapper.Map<PaginationParameters<SearchCriteria>>(queryParams).Returns(mappedParams);
            _mockIsolateSearchRepository.PerformSearchAsync(Arg.Any<PaginationParameters<SearchCriteria>>()).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockIsolateSearchService.PerformSearchAsync(queryParams));
        }

        [Fact]
        public async Task GetIsolateSearchExportResultAsync_ValidInput_ReturnsExpectedOutput()
        {
            // Arrange
            var criteria = new QueryParameters<SearchCriteriaDTO>
            {
                Filter = new SearchCriteriaDTO { FullSearch = false }
            };
            var mappedCriteria = new PaginationParameters<SearchCriteria>();
            var isolateFullDetails = new List<IsolateSearchResult>
            {
                new IsolateSearchResult
                {
                    FreezerName = "F1",
                    TrayName = "T1",
                    Well = "W1"
                }
            };
            var IsolateSearchResultDto = new List<IsolateSearchResultDto>
            {
                new IsolateSearchResultDto
                {
                     FreezerName = "F1",
                     TrayName = "T1",
                     Well = "W1"
                }
            };
            var isolateSearchExportDto = new IsolateSearchExportDto
            {
                Freezer = "F1",
                Tray = "T1",
                Well = "W1"
            };
            var isolateFullDetailsDto = new IsolateFullDetailDto
            {
                IsolateDetails = new IsolateInfoDto
                {
                    FreezerName = "F1",
                    TrayName = "T1",
                    Well = "W1"
                },
                IsolateViabilityDetails = new List<IsolateViabilityInfoDto>
                    {
                        new IsolateViabilityInfoDto
                        {
                            ViabilityStatus = "Viable",
                            CheckedByName = "John Doe",
                            DateChecked = DateTime.Now
                        }
                    },
                IsolateDispatchDetails = new List<IsolateDispatchInfoDto>
                    {
                        new IsolateDispatchInfoDto
                        {
                            RecipientName = "Recp1",
                            RecipientAddress = "RecptAddress"
                        }
                    },
                IsolateCharacteristicDetails = new List<IsolateCharacteristicInfoDto>
                    {
                        new IsolateCharacteristicInfoDto
                        {
                            CharacteristicName = "Color",
                            CharacteristicValue = "Red"
                        }
                    }
            };

            _mockMapper.Map<PaginationParameters<SearchCriteria>>(criteria).Returns(mappedCriteria);
            _mockIsolateSearchRepository.GetIsolateSearchExportResultAsync(mappedCriteria).Returns(isolateFullDetails);
            _mockMapper.Map<IsolateFullDetailDto>(Arg.Any<IsolateFullDetail>()).Returns(isolateFullDetailsDto);
            _mockMapper.Map<IsolateSearchExportDto>(Arg.Any<IsolateInfoDto>()).Returns(isolateSearchExportDto);
            _mockMapper.Map<List<IsolateSearchResultDto>>(Arg.Any<List<IsolateSearchResult>>()).Returns(IsolateSearchResultDto);
            // Act
            var result = await _mockIsolateSearchService.GetIsolateSearchExportResultAsync(criteria);

            // Assert
            Assert.Single(result);
            Assert.Equal("F1", result[0].Freezer);
            Assert.Equal("T1", result[0].Tray);
            Assert.Equal("W1", result[0].Well);
            Assert.Contains("Viable: checked by John Doe on", result[0].ViabilityChecks);
            Assert.Equal("Color: Red", result[0].Characteristics);
        }

        [Fact]
        public async Task GetIsolateSearchExportResultAsync_EmptyCriteria_ReturnsEmptyList()
        {
            // Arrange
            var criteria = new QueryParameters<SearchCriteriaDTO>
            {
                Filter = new SearchCriteriaDTO()
            };
            var mappedCriteria = new PaginationParameters<SearchCriteria>();
            _mockMapper.Map<PaginationParameters<SearchCriteria>>(criteria).Returns(mappedCriteria);
            _mockMapper.Map<List<IsolateFullDetailDto>>(Arg.Any<List<IsolateFullDetail>>()).Returns(new List<IsolateFullDetailDto>());
            _mockIsolateSearchRepository.GetIsolateSearchExportResultAsync(mappedCriteria).Returns(new List<IsolateSearchResult>());
            _mockMapper.Map<List<IsolateSearchResultDto>>(Arg.Any<List<IsolateSearchResult>>()).Returns(new List<IsolateSearchResultDto>());

            // Act
            var result = await _mockIsolateSearchService.GetIsolateSearchExportResultAsync(criteria);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetIsolateSearchExportResultAsync_FullSearchTrue_ExcludesFreezerTrayWell()
        {
            // Arrange
            var criteria = new QueryParameters<SearchCriteriaDTO>
            {
                Filter = new SearchCriteriaDTO { FullSearch = true }
            };
            var mappedCriteria = new PaginationParameters<SearchCriteria>();
            var isolateSearchResult = new List<IsolateSearchResult>
            {
                new IsolateSearchResult
                {
                     FreezerName = "F1",
                     TrayName = "T1",
                     Well = "W1"
                }
            };
            var IsolateSearchResultDto = new List<IsolateSearchResultDto>
            {
                new IsolateSearchResultDto
                {
                     FreezerName = "F1",
                     TrayName = "T1",
                     Well = "W1"
                }
            };
            var isolateSearchExportDto = new IsolateSearchExportDto
            {
                Freezer = "F1",
                Tray = "T1",
                Well = "W1"
            };
            var isolateFullDetailsDto = new IsolateFullDetailDto
            {
                IsolateDetails = new IsolateInfoDto
                {
                    FreezerName = "F1",
                    TrayName = "T1",
                    Well = "W1"
                },
                IsolateViabilityDetails = new List<IsolateViabilityInfoDto>(),
                IsolateDispatchDetails = new List<IsolateDispatchInfoDto>(),
                IsolateCharacteristicDetails = new List<IsolateCharacteristicInfoDto>()
            };

            _mockMapper.Map<PaginationParameters<SearchCriteria>>(criteria).Returns(mappedCriteria);
            _mockMapper.Map<IsolateFullDetailDto>(Arg.Any<IsolateFullDetail>()).Returns(isolateFullDetailsDto);
            _mockMapper.Map<IsolateSearchExportDto>(Arg.Any<IsolateInfoDto>()).Returns(isolateSearchExportDto);
            _mockIsolateSearchRepository.GetIsolateSearchExportResultAsync(mappedCriteria).Returns(isolateSearchResult);
            _mockMapper.Map<List<IsolateSearchResultDto>>(Arg.Any<List<IsolateSearchResult>>()).Returns(IsolateSearchResultDto);
            _mockMapper.Map<IsolateSearchExportDto>(Arg.Any<IsolateSearchExportDto>()).Returns(x => x.Arg<IsolateSearchExportDto>());

            // Act
            var result = await _mockIsolateSearchService.GetIsolateSearchExportResultAsync(criteria);

            // Assert
            Assert.Single(result);
            Assert.Equal("", result[0].Freezer);
            Assert.Equal("", result[0].Tray);
            Assert.Equal("", result[0].Well);
        }

        [Fact]
        public async Task GetIsolateSearchExportResultAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var criteria = new QueryParameters<SearchCriteriaDTO>
            {
                Filter = new SearchCriteriaDTO()
            };
            var mappedCriteria = new PaginationParameters<SearchCriteria>();
            _mockMapper.Map<PaginationParameters<SearchCriteria>>(criteria).Returns(mappedCriteria);
            _mockIsolateSearchRepository.GetIsolateSearchExportResultAsync(mappedCriteria).ThrowsAsync(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockIsolateSearchService.GetIsolateSearchExportResultAsync(criteria));
        }

        [Fact]
        public async Task PerformSearchAsync_WithSingleListAndEquals_SetsCharacteristicValue1()
        {
            // Arrange
            var queryParams = new QueryParameters<SearchCriteriaDTO>
            {
                Filter = new SearchCriteriaDTO
                {
                    CharacteristicSearch = new List<CharacteristicCriteriaDto>
            {
                new CharacteristicCriteriaDto
                {
                    CharacteristicType = "SingleList",
                    Comparator = "=",
                    CharacteristicListValue = "ListVal"
                }
            }
                }
            };

            var mappedParams = new PaginationParameters<SearchCriteria>();
            var searchResult = new PagedData<IsolateSearchResult>(new List<IsolateSearchResult>(), 0);
            var expectedResult = new PaginatedResult<IsolateSearchResultDto>
            {
                data = new List<IsolateSearchResultDto>(),
                TotalCount = 0
            };

            _mockMapper.Map<PaginationParameters<SearchCriteria>>(queryParams).Returns(mappedParams);
            _mockIsolateSearchRepository.PerformSearchAsync(mappedParams).Returns(searchResult);
            _mockMapper.Map<PaginatedResult<IsolateSearchResultDto>>(searchResult).Returns(expectedResult);

            // Act
            var result = await _mockIsolateSearchService.PerformSearchAsync(queryParams);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ListVal", queryParams.Filter.CharacteristicSearch[0].CharacteristicValue1);
        }

        [Fact]
        public async Task PerformSearchAsync_WithSingleListAndNotEqualTo_SetsCharacteristicValue1()
        {
            // Arrange
            var queryParams = new QueryParameters<SearchCriteriaDTO>
            {
                Filter = new SearchCriteriaDTO
                {
                    CharacteristicSearch = new List<CharacteristicCriteriaDto>
            {
                new CharacteristicCriteriaDto
                {
                    CharacteristicType = "SingleList",
                    Comparator = "not equal to",
                    CharacteristicListValue = "AnotherVal"
                }
            }
                }
            };

            var mappedParams = new PaginationParameters<SearchCriteria>();
            var searchResult = new PagedData<IsolateSearchResult>(new List<IsolateSearchResult>(), 0);
            var expectedResult = new PaginatedResult<IsolateSearchResultDto>
            {
                data = new List<IsolateSearchResultDto>(),
                TotalCount = 0
            };

            _mockMapper.Map<PaginationParameters<SearchCriteria>>(queryParams).Returns(mappedParams);
            _mockIsolateSearchRepository.PerformSearchAsync(mappedParams).Returns(searchResult);
            _mockMapper.Map<PaginatedResult<IsolateSearchResultDto>>(searchResult).Returns(expectedResult);

            // Act
            var result = await _mockIsolateSearchService.PerformSearchAsync(queryParams);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("AnotherVal", queryParams.Filter.CharacteristicSearch[0].CharacteristicValue1);
        }

        [Fact]
        public async Task PerformSearchAsync_WithYesNoAndYes_SetsCharacteristicValue1ToTrue()
        {
            // Arrange
            var queryParams = new QueryParameters<SearchCriteriaDTO>
            {
                Filter = new SearchCriteriaDTO
                {
                    CharacteristicSearch = new List<CharacteristicCriteriaDto>
            {
                new CharacteristicCriteriaDto
                {
                    CharacteristicType = "Yes/No",
                    CharacteristicListValue = "Yes"
                }
            }
                }
            };

            var mappedParams = new PaginationParameters<SearchCriteria>();
            var searchResult = new PagedData<IsolateSearchResult>(new List<IsolateSearchResult>(), 0);
            var expectedResult = new PaginatedResult<IsolateSearchResultDto>
            {
                data = new List<IsolateSearchResultDto>(),
                TotalCount = 0
            };

            _mockMapper.Map<PaginationParameters<SearchCriteria>>(queryParams).Returns(mappedParams);
            _mockIsolateSearchRepository.PerformSearchAsync(mappedParams).Returns(searchResult);
            _mockMapper.Map<PaginatedResult<IsolateSearchResultDto>>(searchResult).Returns(expectedResult);

            // Act
            var result = await _mockIsolateSearchService.PerformSearchAsync(queryParams);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("True", queryParams.Filter.CharacteristicSearch[0].CharacteristicValue1);
        }

        [Fact]
        public async Task PerformSearchAsync_WithYesNoAndNo_SetsCharacteristicValue2ToFalse()
        {
            // Arrange
            var queryParams = new QueryParameters<SearchCriteriaDTO>
            {
                Filter = new SearchCriteriaDTO
                {
                    CharacteristicSearch = new List<CharacteristicCriteriaDto>
            {
                new CharacteristicCriteriaDto
                {
                    CharacteristicType = "Yes/No",
                    CharacteristicListValue = "No"
                }
            }
                }
            };

            var mappedParams = new PaginationParameters<SearchCriteria>();
            var searchResult = new PagedData<IsolateSearchResult>(new List<IsolateSearchResult>(), 0);
            var expectedResult = new PaginatedResult<IsolateSearchResultDto>
            {
                data = new List<IsolateSearchResultDto>(),
                TotalCount = 0
            };

            _mockMapper.Map<PaginationParameters<SearchCriteria>>(queryParams).Returns(mappedParams);
            _mockIsolateSearchRepository.PerformSearchAsync(mappedParams).Returns(searchResult);
            _mockMapper.Map<PaginatedResult<IsolateSearchResultDto>>(searchResult).Returns(expectedResult);

            // Act
            var result = await _mockIsolateSearchService.PerformSearchAsync(queryParams);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("False", queryParams.Filter.CharacteristicSearch[0].CharacteristicValue2);
        }
    }
}
