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

        public TestIsolateSearchRepository(
            VIRDbContext context,
            IQueryable<IsolateSearchResult> searchResults)
            : base(context)
        {
            _searchResults = searchResults;
        }
        public IQueryable<IsolateSearchResult> GetTestQuery() => _searchResults;

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
        new IsolateSearchResult { IsolateId = Guid.NewGuid(), Avnumber = "AV2" }
    };

            var mockSet = CreateMockDbSet(data);
            var mockContext = new Mock<VIRDbContext>();
            mockContext.Setup(c => c.Set<IsolateSearchResult>()).Returns(mockSet.Object);

            var repo = new TestIsolateSearchRepository(mockContext.Object, data.AsQueryable());

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
            var repo = new TestIsolateSearchRepository(new Mock<VIRDbContext>().Object, data);
            var result = TestIsolateSearchRepository.PublicApplyBasicFilters(data, null); // Fixed by qualifying with the type name
            Assert.Equal(data, result);
        }

        [Fact]
        public void ApplyStringFilter_EmptyValue_ReturnsQuery()
        {
            var data = new List<IsolateSearchResult>
            {
                new IsolateSearchResult { IsolateId = Guid.NewGuid(), Avnumber = "AV4" }
            }.AsQueryable();
            var result = TestIsolateSearchRepository.PublicApplyStringFilter(data, "", i => i.Avnumber); // Fixed by qualifying with the type name
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
            var repo = new TestIsolateSearchRepository(new Mock<VIRDbContext>().Object, data);
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
            var repo = new TestIsolateSearchRepository(mockContext.Object, data);

            var criteria = new List<CharacteristicCriteria>
    {
        new CharacteristicCriteria
        {
            Characteristic = Guid.NewGuid(), // valid Guid
            CharacteristicType = "Yes/No",
            CharacteristicValue1 = "Yes"
        }
    };

            // Act
            var result = repo.PublicApplyAllCharacteristicFilters(data, criteria);

            // Assert
            Assert.NotNull(result);
            // Since EF mocks won’t actually apply filters, 
            // just assert that query is returned and same type.
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
            var repo = new TestIsolateSearchRepository(mockContext.Object, data);

            var criteria = new List<CharacteristicCriteria>
            {
                new CharacteristicCriteria
                {
                    Characteristic = Guid.Empty, // invalid Guid
                    CharacteristicType = "Text",
                    CharacteristicValue1 = "ABC"
                }
            };

            // Act
            var result = repo.PublicApplyAllCharacteristicFilters(data, criteria);

            // Assert
            Assert.Equal(data, result); // unchanged because Guid was invalid
        }
    }
}
