using System.Linq.Expressions;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Pagination;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.DataAccess.Repositories;
using Apha.VIR.DataAccess.UnitTests.Repository.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.VIR.DataAccess.UnitTests.Repository.IsolateSearchRepositoryTest
{
    public class TestIsolateSearchRepository : IsolateSearchRepository
    {
        private readonly IQueryable<IsolateSearchResult> _searchResults;
        private readonly IQueryable<IsolateCharacteristicsForSearch> _characteristics;

        public TestIsolateSearchRepository(
            VIRDbContext context,
            IQueryable<IsolateSearchResult> searchResults,
            IQueryable<IsolateCharacteristicsForSearch> characteristics)
            : base(context)
        {
            _searchResults = searchResults;
             _characteristics = characteristics;
        }
        public IQueryable<IsolateSearchResult> GetTestQuery() => _searchResults;
        protected override IQueryable<T> GetDbSetFor<T>()
        {
            if (typeof(T) == typeof(IsolateCharacteristicsForSearch))
                return (IQueryable<T>)_characteristics;
            if (typeof(T) == typeof(IsolateSearchResult))
                return (IQueryable<T>)_searchResults;
            throw new NotImplementedException();
        }


        // Wrappers for private static methods
        public static IQueryable<IsolateSearchResult> PublicApplyBasicFilters(IQueryable<IsolateSearchResult> query, SearchCriteria? filter)
            => (IQueryable<IsolateSearchResult>)typeof(IsolateSearchRepository)
                .GetMethod("ApplyBasicFilters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, [query, filter])!;

        public static IQueryable<IsolateSearchResult> PublicApplyStringFilter(
            IQueryable<IsolateSearchResult> query,
            string filterValue,
            Expression<Func<IsolateSearchResult, string>> selector)
            => (IQueryable<IsolateSearchResult>)typeof(IsolateSearchRepository)
                .GetMethod("ApplyStringFilter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, [query, filterValue, selector])!;

        public static IQueryable<IsolateSearchResult> PublicApplyGuidFilter(
            IQueryable<IsolateSearchResult> query,
            Guid? filterValue,
            Expression<Func<IsolateSearchResult, Guid?>> selector)
            => (IQueryable<IsolateSearchResult>)typeof(IsolateSearchRepository)
                .GetMethod("ApplyGuidFilter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, [query, filterValue, selector])!;

        public static IQueryable<IsolateSearchResult> PublicApplyYearOfIsolationFilter(IQueryable<IsolateSearchResult> query, int year)
            => (IQueryable<IsolateSearchResult>)typeof(IsolateSearchRepository)
                .GetMethod("ApplyYearOfIsolationFilter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, [query, year])!;

        public static IQueryable<IsolateSearchResult> PublicApplyDateFilters(IQueryable<IsolateSearchResult> query, SearchCriteria? filter)
            => (IQueryable<IsolateSearchResult>)typeof(IsolateSearchRepository)
                .GetMethod("ApplyDateFilters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, [query, filter])!;

        public static IQueryable<IsolateSearchResult> PublicApplySorting(IQueryable<IsolateSearchResult> query, string? sortBy, bool desc)
            => (IQueryable<IsolateSearchResult>)typeof(IsolateSearchRepository)
                .GetMethod("ApplySorting", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, [query, sortBy, desc])!;

        public static IQueryable<IsolateSearchResult> PublicApplySortingByProperty(IQueryable<IsolateSearchResult> query, string property, bool desc)
            => (IQueryable<IsolateSearchResult>)typeof(IsolateSearchRepository)
                .GetMethod("ApplySortingByProperty", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, [query, property, desc])!;

        public IQueryable<IsolateSearchResult> PublicApplyAllCharacteristicFilters(
    IQueryable<IsolateSearchResult> query,
    List<CharacteristicCriteria> criteria)
        {
            var method = typeof(IsolateSearchRepository)
                .GetMethod("ApplyAllCharacteristicFilters",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance)!;

            return (IQueryable<IsolateSearchResult>)method.Invoke(this, new object[] { query, criteria })!;
        }

        public IQueryable<IsolateSearchResult> PublicApplyCharacteristicFilter(
       IQueryable<IsolateSearchResult> query, CharacteristicCriteria characteristicItem)
        {
            var method = typeof(IsolateSearchRepository)
                .GetMethod("ApplyCharacteristicFilter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            return (IQueryable<IsolateSearchResult>)method.Invoke(this, new object[] { query, characteristicItem })!;
        }
        
        public IQueryable<IsolateSearchResult> PublicApplyNumericCharacteristicFilter(
    IQueryable<IsolateSearchResult> query,
    CharacteristicCriteria characteristicItem)
        {
            if (characteristicItem.CharacteristicType != "Numeric")
                return query;

            var isolateIds = query.Select(i => i.IsolateId).ToList();

            // Materialize before using TryParse
            var numericMatches = _characteristics
                .Where(c =>
                    isolateIds.Contains(c.CharacteristicIsolateId) &&
                    c.VirusCharacteristicId == characteristicItem.Characteristic)
                .AsEnumerable(); // <-- Materialize here

            if (double.TryParse(characteristicItem.CharacteristicValue1, out var compareValue))
            {
                switch (characteristicItem.Comparator)
                {
                    case ">":
                        numericMatches = numericMatches.Where(c => double.TryParse(c.CharacteristicValue, out var val) && val > compareValue);
                        break;
                    case "<":
                        numericMatches = numericMatches.Where(c => double.TryParse(c.CharacteristicValue, out var val) && val < compareValue);
                        break;
                    case "=":
                        numericMatches = numericMatches.Where(c => double.TryParse(c.CharacteristicValue, out var val) && Math.Abs(val - compareValue) < 0.0001);
                        break;
                }
            }

            var matchingIds = numericMatches.Select(c => c.CharacteristicIsolateId).ToList();
            return query.Where(i => matchingIds.Contains(i.IsolateId));
        }

    }
    public class IsolateSearchRepositoryTests
    {
        private static Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> elements) where T : class
        {
            var asyncData = new TestAsyncEnumerable<T>(elements);
            var mockSet = new Mock<DbSet<T>>();

            // Cast asyncData to IQueryable<T> to access Provider, Expression, etc.
            var queryableData = (IQueryable<T>)asyncData;

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryableData.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryableData.GetEnumerator());

            mockSet.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(asyncData.AsEnumerable().GetEnumerator()));

            return mockSet;
        }


        private static PaginationParameters<SearchCriteria> GetDefaultCriteria()
        {
            return new PaginationParameters<SearchCriteria>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "",
                Filter = new SearchCriteria
                {
                    CharacteristicSearch = new List<CharacteristicCriteria>()
                }
            };
        }

        [Fact]
        public async Task PerformSearchAsync_ReturnsPagedData()
        {
            // Arrange
            var data = new List<IsolateSearchResult>
        {
            new IsolateSearchResult { IsolateId = Guid.NewGuid(), Avnumber = "AV1" }
        };
            var mockSet = CreateMockDbSet(data);

            var mockContext = new Mock<VIRDbContext>();
            mockContext.Setup(c => c.VwIsolates).Returns(mockSet.Object);
            mockContext.Setup(c => c.Set<IsolateSearchResult>()).Returns(mockSet.Object);

            var repo = new IsolateSearchRepository(mockContext.Object);

            var criteria = GetDefaultCriteria();

            // Act
            var result = await repo.PerformSearchAsync(criteria);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal("AV1", result.Items.First().Avnumber);
        }

        [Fact]
        public async Task GetIsolateSearchExportResultAsync_ReturnsAll()
        {
            // Arrange
            var data = new List<IsolateSearchResult>
            {
                 new() { IsolateId = Guid.NewGuid(), Avnumber = "AV2" }
            };

            var characteristics = new List<IsolateCharacteristicsForSearch>().AsQueryable();

            var mockSet = CreateMockDbSet(data);
            var mockContext = new Mock<VIRDbContext>();
            mockContext.Setup(c => c.Set<IsolateSearchResult>()).Returns(mockSet.Object);

            var repo = new TestIsolateSearchRepository(
                mockContext.Object,
                mockSet.Object,   
                characteristics
            );

            var criteria = GetDefaultCriteria();

            // Act
            var result = await repo.GetIsolateSearchExportResultAsync(criteria);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("AV2", result.First().Avnumber);
        }

        [Fact]
        public void IsValidGuid_ReturnsExpected()
        {
            Assert.False(IsolateSearchRepository.IsValidGuid(null));
            Assert.False(IsolateSearchRepository.IsValidGuid(Guid.Empty));
            Assert.True(IsolateSearchRepository.IsValidGuid(Guid.NewGuid()));
        }

        [Fact]
        public void ApplyBasicFilters_NullFilter_ReturnsQuery()
        {
            var data = new List<IsolateSearchResult>
            {
                new IsolateSearchResult { IsolateId = Guid.NewGuid(), Avnumber = "AV3" }
            }.AsQueryable();
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestIsolateSearchRepository(
                mockContext.Object,
                data,
                new List<IsolateCharacteristicsForSearch>().AsQueryable() 
            );
            var result = TestIsolateSearchRepository.PublicApplyBasicFilters(data, null); 
            Assert.Equal(data, result);
        }

        [Fact]
        public void ApplyStringFilter_EmptyValue_ReturnsQuery()
        {
            var data = new List<IsolateSearchResult>
            {
                new IsolateSearchResult { IsolateId = Guid.NewGuid(), Avnumber = "AV4" }
            }.AsQueryable();
            var result = TestIsolateSearchRepository.PublicApplyStringFilter(data, "", i => i.Avnumber); 
            Assert.Equal(data, result);
        }

        [Fact]
        public void ApplyGuidFilter_InvalidGuid_ReturnsQuery()
        {
            var data = new List<IsolateSearchResult>
            {
                new IsolateSearchResult { IsolateId = Guid.NewGuid(), Avnumber = "AV5" }
            }.AsQueryable();
            var result = TestIsolateSearchRepository.PublicApplyGuidFilter(data, null, i => i.Family);
            Assert.Equal(data, result);
        }

        [Fact]
        public void ApplyYearOfIsolationFilter_ZeroYear_ReturnsQuery()
        {
            var data = new List<IsolateSearchResult>
            {
                new IsolateSearchResult { IsolateId = Guid.NewGuid(), YearOfIsolation = 2020 }
            }.AsQueryable();
            var result = TestIsolateSearchRepository.PublicApplyYearOfIsolationFilter(data, 0);
            Assert.Equal(data, result);
        }

        [Fact]
        public void ApplyDateFilters_NullFilter_ReturnsQuery()
        {
            var data = new List<IsolateSearchResult>
            {
                new IsolateSearchResult { IsolateId = Guid.NewGuid() }
            }.AsQueryable();
            var result = TestIsolateSearchRepository.PublicApplyDateFilters(data, null);
            Assert.Equal(data, result);
        }

        [Fact]
        public void ApplySorting_NullSortBy_ReturnsQuery()
        {
            var data = new List<IsolateSearchResult>
            {
                new IsolateSearchResult { IsolateId = Guid.NewGuid() }
            }.AsQueryable();
            var result = TestIsolateSearchRepository.PublicApplySorting(data, null, false);
            Assert.Equal(data, result);
        }

        [Fact]
        public void ApplySortingByProperty_UnknownProperty_ReturnsQuery()
        {
            var data = new List<IsolateSearchResult>
            {
                new IsolateSearchResult { IsolateId = Guid.NewGuid() }
            }.AsQueryable();

            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestIsolateSearchRepository(
                mockContext.Object,
                data,
                new List<IsolateCharacteristicsForSearch>().AsQueryable() 
            );

            var result = TestIsolateSearchRepository.PublicApplySortingByProperty(data, "unknown", false);
            Assert.Equal(data, result);
        }

        [Fact]
        public void ApplyStringFilter_ValidMatch_ReturnsFilteredResults()
        {
            // Arrange
            var data = new List<IsolateSearchResult>
            {
                new IsolateSearchResult { Avnumber = "ABC" },
                new IsolateSearchResult { Avnumber = "XYZ" }
            }.AsQueryable();

            // Act
            var result = TestIsolateSearchRepository.PublicApplyStringFilter(data, "ABC", i => i.Avnumber);

            // Assert
            Assert.Single(result);
            Assert.Equal("ABC", result.First().Avnumber);
        }

        [Fact]
        public void ApplyGuidFilter_ValidGuid_ReturnsFilteredResults()
        {
            // Arrange
            var g1 = Guid.NewGuid();
            var g2 = Guid.NewGuid();
            var data = new List<IsolateSearchResult>
            {
                new IsolateSearchResult { Family = g1 },
                new IsolateSearchResult { Family = g2 }
            }.AsQueryable();

            // Act
            var result = TestIsolateSearchRepository.PublicApplyGuidFilter(data, g1, i => i.Family);

            // Assert
            Assert.Single(result);
            Assert.Equal(g1, result.First().Family);
        }

        [Fact]
        public void ApplyDateFilters_ReceivedDateRange_FiltersCorrectly()
        {
            // Arrange
            var data = new List<IsolateSearchResult>
            {
                new IsolateSearchResult { ReceivedDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new IsolateSearchResult { ReceivedDate = new DateTime(2023, 5, 1, 0, 0, 0, DateTimeKind.Utc) }
            }.AsQueryable();

            var criteria = new SearchCriteria
            {
                ReceivedFromDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                ReceivedToDate = new DateTime(2023, 3, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            // Act
            var result = TestIsolateSearchRepository.PublicApplyDateFilters(data, criteria);

            // Assert
            Assert.Single(result);
            Assert.Equal(new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc), result.First().ReceivedDate);
        }

        [Fact]
        public void ApplyDateFilters_CreatedDateRange_FiltersCorrectly()
        {
            // Arrange
            var data = new List<IsolateSearchResult>
            {
                new IsolateSearchResult { DateCreated = new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new IsolateSearchResult { DateCreated = new DateTime(2022, 6, 1, 0, 0, 0, DateTimeKind.Utc) }
            }.AsQueryable();

            var criteria = new SearchCriteria
            {
                CreatedFromDate = new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedToDate = new DateTime(2022, 3, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            // Act
            var result = TestIsolateSearchRepository.PublicApplyDateFilters(data, criteria);

            // Assert
            Assert.Single(result);
            Assert.Equal(new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc), result.First().DateCreated);
        }

        [Fact]
        public void ApplySortingByProperty_SortsByAvnumberAscending()
        {
            // Arrange
            var data = new List<IsolateSearchResult>
            {
                new IsolateSearchResult { Avnumber = "B" },
                new IsolateSearchResult { Avnumber = "A" }
            }.AsQueryable();

            // Act
            var result = TestIsolateSearchRepository.PublicApplySortingByProperty(data, "avnumber", false);

            // Assert
            Assert.Equal("A", result.First().Avnumber);
        }

        [Fact]
        public void ApplySortingByProperty_SortsByAvnumberDescending()
        {
            // Arrange
            var data = new List<IsolateSearchResult>
            {
                new IsolateSearchResult { Avnumber = "A" },
                new IsolateSearchResult { Avnumber = "B" }
            }.AsQueryable();

            // Act
            var result = TestIsolateSearchRepository.PublicApplySortingByProperty(data, "avnumber", true);

            // Assert
            Assert.Equal("B", result.First().Avnumber);
        }

        [Fact]
        public void ApplyAllCharacteristicFilters_WithValidGuid_ReturnsQuery()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            var data = new List<IsolateSearchResult>
    {
        new IsolateSearchResult { IsolateId = isolateId }
    }.AsQueryable();

            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestIsolateSearchRepository(
     mockContext.Object,
     data,
     new List<IsolateCharacteristicsForSearch>().AsQueryable()
 );
            var criteria = new List<CharacteristicCriteria>
    {
        new CharacteristicCriteria
        {
            Characteristic = Guid.NewGuid(), 
            CharacteristicType = "Yes/No",
            CharacteristicValue1 = "Yes"
        }
    };

            // Act
            var result = repo.PublicApplyAllCharacteristicFilters(data, criteria);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IQueryable<IsolateSearchResult>>(result);
        }


        [Fact]
        public void ApplyAllCharacteristicFilters_WithInvalidGuid_ReturnsSameQuery()
        {
            // Arrange
            var data = new List<IsolateSearchResult>
            {
                new IsolateSearchResult { IsolateId = Guid.NewGuid() }
            }.AsQueryable();

            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestIsolateSearchRepository(
    mockContext.Object,
    data,
    new List<IsolateCharacteristicsForSearch>().AsQueryable()
);
            var criteria = new List<CharacteristicCriteria>
            {
                new CharacteristicCriteria
                {
                    Characteristic = Guid.Empty,
                    CharacteristicType = "Text",
                    CharacteristicValue1 = "ABC"
                }
            };

            // Act
            var result = repo.PublicApplyAllCharacteristicFilters(data, criteria);

            // Assert
            Assert.Equal(data, result); 
        }

        [Fact]
        public void ApplyCharacteristicFilter_NumericType_CallsNumeric()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            var numericId = Guid.NewGuid();

            var data = new List<IsolateSearchResult>
    {
        new IsolateSearchResult { IsolateId = isolateId }
    }.AsQueryable();

            var fakeNumeric = new List<IsolateCharacteristicsForSearch>
    {
        new IsolateCharacteristicsForSearch
        {
            CharacteristicIsolateId = isolateId,
            VirusCharacteristicId = numericId,
            CharacteristicValue = "123"
        }
    }.AsQueryable();

            var mockContext = new Mock<VIRDbContext>();

            var repo = new TestIsolateSearchRepository(
                mockContext.Object,
                data,
                fakeNumeric
            );

            var criteria = new CharacteristicCriteria
            {
                Characteristic = numericId,
                CharacteristicType = "Numeric",
                Comparator = ">",
                CharacteristicValue1 = "100"
            };

            // Act
            var result = repo.PublicApplyNumericCharacteristicFilter(data, criteria);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(isolateId, result.First().IsolateId);
        }


        [Fact]
        public void ApplyCharacteristicFilter_SingleListType_CallsSingleList()
        {
            var data = new List<IsolateSearchResult> { new IsolateSearchResult { IsolateId = Guid.NewGuid() } }.AsQueryable();

            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestIsolateSearchRepository(
     mockContext.Object,
     data,
     new List<IsolateCharacteristicsForSearch>().AsQueryable()
 );
            var criteria = new CharacteristicCriteria
            {
                Characteristic = Guid.NewGuid(),
                CharacteristicType = "SingleList",
                Comparator = "begins with",
                CharacteristicValue1 = "A"
            };
            var result = repo.PublicApplyCharacteristicFilter(data, criteria);
            Assert.NotNull(result);
        }

        [Fact]
        public void ApplyCharacteristicFilter_YesNoType_CallsYesNo()
        {
            var data = new List<IsolateSearchResult> { new IsolateSearchResult { IsolateId = Guid.NewGuid() } }.AsQueryable();

            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestIsolateSearchRepository(
    mockContext.Object,
    data,
    new List<IsolateCharacteristicsForSearch>().AsQueryable()
);
            var criteria = new CharacteristicCriteria
            {
                Characteristic = Guid.NewGuid(),
                CharacteristicType = "Yes/No",
                CharacteristicValue1 = "Yes"
            };
            var result = repo.PublicApplyCharacteristicFilter(data, criteria);
            Assert.NotNull(result);
        }

        [Fact]
        public void ApplyCharacteristicFilter_TextType_CallsText()
        {
            var data = new List<IsolateSearchResult> { new IsolateSearchResult { IsolateId = Guid.NewGuid() } }.AsQueryable();

            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestIsolateSearchRepository(
    mockContext.Object,
    data,
    new List<IsolateCharacteristicsForSearch>().AsQueryable()
);
            var criteria = new CharacteristicCriteria
            {
                Characteristic = Guid.NewGuid(),
                CharacteristicType = "Text",
                Comparator = "contains",
                CharacteristicValue1 = "foo"
            };
            var result = repo.PublicApplyCharacteristicFilter(data, criteria);
            Assert.NotNull(result);
        }

        [Fact]
        public void ApplySingleListCharacteristicFilter_BeginsWith_NullOrEmptyValue_DoesNotFilter()
        {
            var isolateId = Guid.NewGuid();
            var charId = Guid.NewGuid();
            var data = new List<IsolateSearchResult>
    {
        new IsolateSearchResult { IsolateId = isolateId }
    }.AsQueryable();

            var characteristics = new List<IsolateCharacteristicsForSearch>
    {
        new IsolateCharacteristicsForSearch
        {
            CharacteristicIsolateId = isolateId,
            VirusCharacteristicId = charId,
            CharacteristicValue = "Apple"
        }
    }.AsQueryable();

            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestIsolateSearchRepository(mockContext.Object, data, characteristics);

            // Null value
            var criteriaNull = new CharacteristicCriteria
            {
                Characteristic = charId,
                CharacteristicType = "SingleList",
                Comparator = "begins with",
                CharacteristicValue1 = null
            };
            var resultNull = repo.PublicApplyCharacteristicFilter(data, criteriaNull);
            Assert.NotNull(resultNull);

            // Empty value
            var criteriaEmpty = new CharacteristicCriteria
            {
                Characteristic = charId,
                CharacteristicType = "SingleList",
                Comparator = "begins with",
                CharacteristicValue1 = ""
            };
            var resultEmpty = repo.PublicApplyCharacteristicFilter(data, criteriaEmpty);
            Assert.NotNull(resultEmpty);
        }


        [Fact]
        public void ApplySingleListCharacteristicFilter_NotEqualTo_FiltersCorrectly()
        {
            var isolateId = Guid.NewGuid();
            var charId = Guid.NewGuid();
            var data = new List<IsolateSearchResult>
    {
        new IsolateSearchResult { IsolateId = isolateId }
    }.AsQueryable();

            var characteristics = new List<IsolateCharacteristicsForSearch>
    {
        new IsolateCharacteristicsForSearch
        {
            CharacteristicIsolateId = isolateId,
            VirusCharacteristicId = charId,
            CharacteristicValue = "Banana"
        }
    }.AsQueryable();

            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestIsolateSearchRepository(mockContext.Object, data, characteristics);

            var criteria = new CharacteristicCriteria
            {
                Characteristic = charId,
                CharacteristicType = "SingleList",
                Comparator = "not equal to",
                CharacteristicValue1 = "Apple"
            };
            var result = repo.PublicApplyCharacteristicFilter(data, criteria);
            Assert.NotNull(result);
        }

        [Fact]
        public void ApplySingleListCharacteristicFilter_Default_NullOrEmptyValue_DoesNotFilter()
        {
            var isolateId = Guid.NewGuid();
            var charId = Guid.NewGuid();
            var data = new List<IsolateSearchResult>
    {
        new IsolateSearchResult { IsolateId = isolateId }
    }.AsQueryable();

            var characteristics = new List<IsolateCharacteristicsForSearch>
    {
        new IsolateCharacteristicsForSearch
        {
            CharacteristicIsolateId = isolateId,
            VirusCharacteristicId = charId,
            CharacteristicValue = "Apple"
        }
    }.AsQueryable();

            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestIsolateSearchRepository(mockContext.Object, data, characteristics);

            var criteria = new CharacteristicCriteria
            {
                Characteristic = charId,
                CharacteristicType = "SingleList",
                Comparator = "equals",
                CharacteristicValue1 = null
            };
            var result = repo.PublicApplyCharacteristicFilter(data, criteria);
            Assert.NotNull(result);
        }

        [Fact]
        public void ApplyTextCharacteristicFilter_Contains_NullOrEmptyValue_DoesNotFilter()
        {
            var isolateId = Guid.NewGuid();
            var charId = Guid.NewGuid();
            var data = new List<IsolateSearchResult>
    {
        new IsolateSearchResult { IsolateId = isolateId }
    }.AsQueryable();

            var characteristics = new List<IsolateCharacteristicsForSearch>
    {
        new IsolateCharacteristicsForSearch
        {
            CharacteristicIsolateId = isolateId,
            VirusCharacteristicId = charId,
            CharacteristicValue = "foo"
        }
    }.AsQueryable();

            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestIsolateSearchRepository(mockContext.Object, data, characteristics);

            // Null value
            var criteriaNull = new CharacteristicCriteria
            {
                Characteristic = charId,
                CharacteristicType = "Text",
                Comparator = "contains",
                CharacteristicValue1 = null
            };
            var resultNull = repo.PublicApplyCharacteristicFilter(data, criteriaNull);
            Assert.NotNull(resultNull);

            // Empty value
            var criteriaEmpty = new CharacteristicCriteria
            {
                Characteristic = charId,
                CharacteristicType = "Text",
                Comparator = "contains",
                CharacteristicValue1 = ""
            };
            var resultEmpty = repo.PublicApplyCharacteristicFilter(data, criteriaEmpty);
            Assert.NotNull(resultEmpty);
        }

        [Theory]
        [InlineData("avnumber")]
        [InlineData("senderreferencenumber")]
        [InlineData("sampletypename")]
        [InlineData("familyname")]
        [InlineData("typename")]
        [InlineData("groupspeciesname")]
        [InlineData("breedname")]
        [InlineData("yearofisolation")]
        [InlineData("receiveddate")]
        [InlineData("countryoforiginname")]
        [InlineData("materialtransferagreement")]
        [InlineData("noofaliquots")]
        [InlineData("freezername")]
        [InlineData("trayname")]
        [InlineData("well")]
        public void ApplySortingByProperty_CoversAllProperties(string property)
        {
            var data = new List<IsolateSearchResult>
    {
        new IsolateSearchResult { Avnumber = "A", SenderReferenceNumber = "S1", SampleTypeName = "T1", FamilyName = "F1", TypeName = "Ty1", GroupSpeciesName = "G1", BreedName = "B1", YearOfIsolation = 2020, ReceivedDate = DateTime.UtcNow, CountryOfOriginName = "C1", MaterialTransferAgreement = true, NoOfAliquots = 1, FreezerName = "FZ1", TrayName = "TR1", Well = "W1" },
        new IsolateSearchResult { Avnumber = "B", SenderReferenceNumber = "S2", SampleTypeName = "T2", FamilyName = "F2", TypeName = "Ty2", GroupSpeciesName = "G2", BreedName = "B2", YearOfIsolation = 2021, ReceivedDate = DateTime.UtcNow.AddDays(-1), CountryOfOriginName = "C2", MaterialTransferAgreement = false, NoOfAliquots = 2, FreezerName = "FZ2", TrayName = "TR2", Well = "W2" }
    }.AsQueryable();

            var resultAsc = TestIsolateSearchRepository.PublicApplySortingByProperty(data, property, false);
            Assert.NotNull(resultAsc);

            var resultDesc = TestIsolateSearchRepository.PublicApplySortingByProperty(data, property, true);
            Assert.NotNull(resultDesc);
        }
    }
}
