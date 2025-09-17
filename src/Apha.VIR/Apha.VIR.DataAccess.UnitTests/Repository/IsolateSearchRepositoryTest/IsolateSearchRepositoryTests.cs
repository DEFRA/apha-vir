using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
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


        // Test helper to expose data directly
        public IQueryable<IsolateSearchResult> GetTestQuery() => _searchResults;

        // Wrappers for private static methods
        public IQueryable<IsolateSearchResult> PublicApplyBasicFilters(IQueryable<IsolateSearchResult> query, SearchCriteria? filter)
            => (IQueryable<IsolateSearchResult>)typeof(IsolateSearchRepository)
                .GetMethod("ApplyBasicFilters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object?[] { query, filter })!;

        public IQueryable<IsolateSearchResult> PublicApplyStringFilter(
            IQueryable<IsolateSearchResult> query,
            string filterValue,
            Expression<Func<IsolateSearchResult, string>> selector)
            => (IQueryable<IsolateSearchResult>)typeof(IsolateSearchRepository)
                .GetMethod("ApplyStringFilter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object?[] { query, filterValue, selector })!;

        public IQueryable<IsolateSearchResult> PublicApplyGuidFilter(
            IQueryable<IsolateSearchResult> query,
            Guid? filterValue,
            Expression<Func<IsolateSearchResult, Guid?>> selector)
            => (IQueryable<IsolateSearchResult>)typeof(IsolateSearchRepository)
                .GetMethod("ApplyGuidFilter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object?[] { query, filterValue, selector })!;

        public IQueryable<IsolateSearchResult> PublicApplyYearOfIsolationFilter(IQueryable<IsolateSearchResult> query, int year)
            => (IQueryable<IsolateSearchResult>)typeof(IsolateSearchRepository)
                .GetMethod("ApplyYearOfIsolationFilter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object?[] { query, year })!;

        public IQueryable<IsolateSearchResult> PublicApplyDateFilters(IQueryable<IsolateSearchResult> query, SearchCriteria? filter)
            => (IQueryable<IsolateSearchResult>)typeof(IsolateSearchRepository)
                .GetMethod("ApplyDateFilters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object?[] { query, filter })!;

        public IQueryable<IsolateSearchResult> PublicApplySorting(IQueryable<IsolateSearchResult> query, string? sortBy, bool desc)
            => (IQueryable<IsolateSearchResult>)typeof(IsolateSearchRepository)
                .GetMethod("ApplySorting", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object?[] { query, sortBy, desc })!;

        public IQueryable<IsolateSearchResult> PublicApplySortingByProperty(IQueryable<IsolateSearchResult> query, string property, bool desc)
            => (IQueryable<IsolateSearchResult>)typeof(IsolateSearchRepository)
                .GetMethod("ApplySortingByProperty", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object?[] { query, property, desc })!;
    }
    public class IsolateSearchRepositoryTests
    {
        //private static Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> elements) where T : class
        //{
        //    var queryable = elements.AsQueryable();

        //    var mockSet = new Mock<DbSet<T>>();
        //    mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        //    mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        //    mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        //    mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

        //    mockSet.As<IAsyncEnumerable<T>>()
        //        .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
        //        .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

        //    return mockSet;
        //}
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
                SortBy = "", // Add this line
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
            var result = repo.PublicApplyBasicFilters(data, null);
            Assert.Equal(data, result);
        }

        [Fact]
        public void ApplyStringFilter_EmptyValue_ReturnsQuery()
        {
            var data = new List<IsolateSearchResult>
            {
                new IsolateSearchResult { IsolateId = Guid.NewGuid(), Avnumber = "AV4" }
            }.AsQueryable();
            var repo = new TestIsolateSearchRepository(new Mock<VIRDbContext>().Object, data);
            var result = repo.PublicApplyStringFilter(data, "", i => i.Avnumber);
            Assert.Equal(data, result);
        }

        [Fact]
        public void ApplyGuidFilter_InvalidGuid_ReturnsQuery()
        {
            var data = new List<IsolateSearchResult>
            {
                new IsolateSearchResult { IsolateId = Guid.NewGuid(), Avnumber = "AV5" }
            }.AsQueryable();
            var repo = new TestIsolateSearchRepository(new Mock<VIRDbContext>().Object, data);
            var result = repo.PublicApplyGuidFilter(data, null, i => i.Family);
            Assert.Equal(data, result);
        }

        [Fact]
        public void ApplyYearOfIsolationFilter_ZeroYear_ReturnsQuery()
        {
            var data = new List<IsolateSearchResult>
            {
                new IsolateSearchResult { IsolateId = Guid.NewGuid(), YearOfIsolation = 2020 }
            }.AsQueryable();
            var repo = new TestIsolateSearchRepository(new Mock<VIRDbContext>().Object, data);
            var result = repo.PublicApplyYearOfIsolationFilter(data, 0);
            Assert.Equal(data, result);
        }

        [Fact]
        public void ApplyDateFilters_NullFilter_ReturnsQuery()
        {
            var data = new List<IsolateSearchResult>
            {
                new IsolateSearchResult { IsolateId = Guid.NewGuid() }
            }.AsQueryable();
            var repo = new TestIsolateSearchRepository(new Mock<VIRDbContext>().Object, data);
            var result = repo.PublicApplyDateFilters(data, null);
            Assert.Equal(data, result);
        }

        [Fact]
        public void ApplySorting_NullSortBy_ReturnsQuery()
        {
            var data = new List<IsolateSearchResult>
            {
                new IsolateSearchResult { IsolateId = Guid.NewGuid() }
            }.AsQueryable();
            var repo = new TestIsolateSearchRepository(new Mock<VIRDbContext>().Object, data);
            var result = repo.PublicApplySorting(data, null, false);
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
            var result = repo.PublicApplySortingByProperty(data, "unknown", false);
            Assert.Equal(data, result);
        }
    }
}
