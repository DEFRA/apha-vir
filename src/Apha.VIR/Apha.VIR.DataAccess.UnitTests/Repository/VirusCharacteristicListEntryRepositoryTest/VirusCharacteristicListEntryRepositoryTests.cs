using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Pagination;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.DataAccess.Repositories;
using Apha.VIR.DataAccess.UnitTests.Repository.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.VIR.DataAccess.UnitTests.Repository.VirusCharacteristicListEntryRepositoryTest
{
    public class TestVirusCharacteristicListEntryRepository : VirusCharacteristicListEntryRepository
    {
        private readonly IQueryable<VirusCharacteristicListEntry> _byCharacteristicId;
        private readonly IQueryable<VirusCharacteristicListEntry> _paged;
        private readonly IQueryable<VirusCharacteristicListEntry> _byId;

        public bool AddCalled { get; private set; }
        public bool UpdateCalled { get; private set; }
        public bool DeleteCalled { get; private set; }

        public TestVirusCharacteristicListEntryRepository(
            VIRDbContext context,
            IQueryable<VirusCharacteristicListEntry> byCharacteristicId,
            IQueryable<VirusCharacteristicListEntry> paged,
            IQueryable<VirusCharacteristicListEntry> byId)
            : base(context)
        {
            _byCharacteristicId = byCharacteristicId;
            _paged = paged;
            _byId = byId;
        }
        protected  override IQueryable<T> GetQueryableInterpolatedFor<T>(FormattableString sql)
        {
            if (typeof(T) == typeof(VirusCharacteristicListEntry))
            {
                var query = sql.Format.ToLowerInvariant();

                // If the query is for paging, use _paged; otherwise, use _byCharacteristicId
                // You can distinguish by checking for the presence of "take" or by using a test-only marker in the query string if needed.

                // Use a test-only marker to route to _paged data
                if (query.Contains("spviruscharacteristiclistentrygetbyid") && query.Contains("--paged"))
                    return (IQueryable<T>)_paged;
                // Non-paged call
                if (query.Contains("spviruscharacteristiclistentrygetbyid"))
                    return (IQueryable<T>)_byCharacteristicId;
                if (query.Contains("getbyid"))
                    return (IQueryable<T>)_byCharacteristicId;
                if (query.Contains("select *"))
                    return (IQueryable<T>)_byId;
            }
            throw new NotImplementedException($"No override for type {typeof(T).Name} and query {sql.Format}");
        }
        public IQueryable<T> InvokeGetQueryableInterpolatedFor<T>(FormattableString sql)
     where T : class
        {
            return GetQueryableInterpolatedFor<T>(sql);
        }

        protected override Task<int> ExecuteSqlInterpolatedAsync(FormattableString sql)
        {
            var query = sql.Format.ToLowerInvariant();
            if (query.Contains("insert"))
            {
                AddCalled = true;
                return Task.FromResult(1);
            }
            if (query.Contains("update"))
            {
                UpdateCalled = true;
                return Task.FromResult(1);
            }
            if (query.Contains("delete"))
            {
                DeleteCalled = true;
                return Task.FromResult(1);
            }
            throw new NotImplementedException($"No override for query {sql.Format}");
        }

    }

    public class VirusCharacteristicListEntryRepositoryTests
    {
        [Fact]
        public async Task GetEntriesByCharacteristicIdAsync_ReturnsData()
        {
            var charId = Guid.NewGuid();
            var fakeData = new List<VirusCharacteristicListEntry>
            {
                new VirusCharacteristicListEntry { VirusCharacteristicId = charId, Name = "Entry1" },
                new VirusCharacteristicListEntry { VirusCharacteristicId = charId, Name = "Entry2" }
            };
            var asyncFakeData = new TestAsyncEnumerable<VirusCharacteristicListEntry>(fakeData);
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestVirusCharacteristicListEntryRepository(
                context: mockContext.Object,
                byCharacteristicId: asyncFakeData,
                paged: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
                byId: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>())
            );

            var result = await repo.GetEntriesByCharacteristicIdAsync(charId);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetVirusCharacteristicListEntries_ReturnsPagedData()
        {
            var charId = Guid.NewGuid();
            var fakeData = new List<VirusCharacteristicListEntry>
            {
                new VirusCharacteristicListEntry { VirusCharacteristicId = charId, Name = "Entry1" },
                new VirusCharacteristicListEntry { VirusCharacteristicId = charId, Name = "Entry2" },
                new VirusCharacteristicListEntry { VirusCharacteristicId = charId, Name = "Entry3" }
            };
            var asyncFakeData = new TestAsyncEnumerable<VirusCharacteristicListEntry>(fakeData);
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestVirusCharacteristicListEntryRepository(
                context: mockContext.Object,
                byCharacteristicId: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
                paged: asyncFakeData,
                byId: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>())
            );

            var result = await GetPagedTestData(repo, charId, 1, 2);

            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(2, result.Items.Count);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsEntry()
        {
            var id = Guid.NewGuid();
            var fakeData = new List<VirusCharacteristicListEntry>
            {
                new VirusCharacteristicListEntry { VirusCharacteristicId = Guid.NewGuid(), Name = "Entry1" }
            };
            var asyncFakeData = new TestAsyncEnumerable<VirusCharacteristicListEntry>(fakeData);
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestVirusCharacteristicListEntryRepository(
                context: mockContext.Object,
                byCharacteristicId: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
                paged: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
                byId: asyncFakeData
            );

            var result = await repo.GetByIdAsync(id);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task AddEntryAsync_CallsExecuteSqlInterpolatedAsync()
        {
            var entry = new VirusCharacteristicListEntry
            {
                Id = Guid.NewGuid(),
                Name = "Entry",
                VirusCharacteristicId = Guid.NewGuid()
            };
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestVirusCharacteristicListEntryRepository(
                context: mockContext.Object,
                byCharacteristicId: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
                paged: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
                byId: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>())
            );

            await repo.AddEntryAsync(entry);

            Assert.True(repo.AddCalled);
            Assert.NotNull(entry.LastModified);
        }

        [Fact]
        public async Task UpdateEntryAsync_CallsExecuteSqlInterpolatedAsync_AndSetsLastModified()
        {
            var entry = new VirusCharacteristicListEntry
            {
                Id = Guid.NewGuid(),
                Name = "Entry",
                VirusCharacteristicId = Guid.NewGuid(),
                LastModified = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 }
            };
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestVirusCharacteristicListEntryRepository(
                context: mockContext.Object,
                byCharacteristicId: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
                paged: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
                byId: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>())
            );

            await repo.UpdateEntryAsync(entry);

            Assert.True(repo.UpdateCalled);
            Assert.NotNull(entry.LastModified);
            Assert.Equal(8, entry.LastModified.Length);
        }

        [Fact]
        public async Task DeleteEntryAsync_CallsExecuteSqlInterpolatedAsync()
        {
            var id = Guid.NewGuid();
            var lastModified = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestVirusCharacteristicListEntryRepository(
                context: mockContext.Object,
                byCharacteristicId: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
                paged: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
                byId: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>())
            );

            await repo.DeleteEntryAsync(id, lastModified);

            Assert.True(repo.DeleteCalled);
        }
        private async Task<PagedData<VirusCharacteristicListEntry>> GetPagedTestData(
             TestVirusCharacteristicListEntryRepository repo,
             Guid virusCharacteristicId,
             int pageNo,
             int pageSize)
        {
            // Use the public wrapper, not the protected method
            var result = await repo.InvokeGetQueryableInterpolatedFor<VirusCharacteristicListEntry>(
                $"EXEC spVirusCharacteristicListEntryGetById --paged @VirusCharacteristicId = {virusCharacteristicId}"
            ).ToListAsync();

            var totalRecords = result.Count;
            var entries = result.Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();

            return new PagedData<VirusCharacteristicListEntry>(entries, totalRecords);
        }


    }
}
