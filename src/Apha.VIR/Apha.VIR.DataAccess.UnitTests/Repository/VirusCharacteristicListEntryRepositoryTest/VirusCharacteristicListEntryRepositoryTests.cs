using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Pagination;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.DataAccess.Repositories;
using Apha.VIR.DataAccess.UnitTests.Repository.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Apha.VIR.DataAccess.UnitTests.Repository.VirusCharacteristicListEntryRepositoryTest
{
    public class TestVirusCharacteristicListEntryRepository : VirusCharacteristicListEntryRepository
    {
        private readonly IQueryable<VirusCharacteristicListEntry> _byCharacteristicId;
        private readonly IQueryable<VirusCharacteristicListEntry> _paged;
        private readonly IQueryable<VirusCharacteristicListEntry> _byId;
        private readonly IQueryable<VirusCharacteristicListEntry> _entries;

        public bool AddCalled { get; private set; }
        public bool UpdateCalled { get; private set; }
        public bool DeleteCalled { get; private set; }
       

        public TestVirusCharacteristicListEntryRepository(
            VIRDbContext context,
            IQueryable<VirusCharacteristicListEntry> byCharacteristicId,
            IQueryable<VirusCharacteristicListEntry> paged,
            IQueryable<VirusCharacteristicListEntry> byId,
             IQueryable<VirusCharacteristicListEntry> entries)
            : base(context)
        {
            _byCharacteristicId = byCharacteristicId;
            _paged = paged;
            _byId = byId;
            _entries = entries;
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
                if (typeof(T) == typeof(VirusCharacteristicListEntry))
                {
                    return (IQueryable<T>)_entries;
                }
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
                byId: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
    entries: fakeData.AsQueryable()
            );

            var result = await repo.GetEntriesByCharacteristicIdAsync(charId);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetVirusCharacteristicListEntries_ReturnsPagedData()
        {
            // Arrange
            var characteristicId = Guid.NewGuid();
            var entries = new List<VirusCharacteristicListEntry>
        {
            new VirusCharacteristicListEntry { Id = Guid.NewGuid(), VirusCharacteristicId = characteristicId },
            new VirusCharacteristicListEntry { Id = Guid.NewGuid(), VirusCharacteristicId = characteristicId },
            new VirusCharacteristicListEntry { Id = Guid.NewGuid(), VirusCharacteristicId = characteristicId },
            new VirusCharacteristicListEntry { Id = Guid.NewGuid(), VirusCharacteristicId = characteristicId },
            new VirusCharacteristicListEntry { Id = Guid.NewGuid(), VirusCharacteristicId = characteristicId }
        };
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestVirusCharacteristicListEntryRepository(
                context: mockContext.Object,
                byCharacteristicId: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
                paged: new TestAsyncEnumerable<VirusCharacteristicListEntry>(entries),
                byId: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
        entries: entries.AsQueryable()
            );

            // Act
            var result = await repo.GetVirusCharacteristicListEntries(characteristicId, pageNo: 2, pageSize: 2);

            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
           
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsEntry()
        {
            // Arrange
            var id = Guid.NewGuid();
            var fakeData = new List<VirusCharacteristicListEntry>
    {
        new VirusCharacteristicListEntry { Id = id, VirusCharacteristicId = Guid.NewGuid(), Name = "Entry1" }
    };
            var asyncFakeData = new TestAsyncEnumerable<VirusCharacteristicListEntry>(fakeData);
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestVirusCharacteristicListEntryRepository(
                context: mockContext.Object,
                byCharacteristicId: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
                paged: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
                byId: asyncFakeData,
                entries: fakeData.AsQueryable()
            );

            // Act
            var result = await repo.GetByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal("Entry1", result.Name);
        }

        [Fact]
        public async Task AddEntryAsync_CallsExecuteSqlInterpolatedAsync()
        {
            // Arrange
            var entry = new VirusCharacteristicListEntry
            {
                Id = Guid.NewGuid(),
                Name = "Entry",
                VirusCharacteristicId = Guid.NewGuid()
            };

            var entries = new List<VirusCharacteristicListEntry> { entry };

            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestVirusCharacteristicListEntryRepository(
                context: mockContext.Object,
                byCharacteristicId: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
                paged: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
                byId: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
                entries: entries.AsQueryable()
            );

            // Act
            await repo.AddEntryAsync(entry);

            // Assert
            Assert.True(repo.AddCalled);
            Assert.NotNull(entry.LastModified);
        }

        [Fact]
        public async Task UpdateEntryAsync_CallsExecuteSqlInterpolatedAsync_AndSetsLastModified()
        {
            // Arrange
            var entry = new VirusCharacteristicListEntry
            {
                Id = Guid.NewGuid(),
                Name = "Entry",
                VirusCharacteristicId = Guid.NewGuid(),
                LastModified = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 }
            };

            var entries = new List<VirusCharacteristicListEntry> { entry };

            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestVirusCharacteristicListEntryRepository(
                context: mockContext.Object,
                byCharacteristicId: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
                paged: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
                byId: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
                entries: entries.AsQueryable()
            );

            // Act
            await repo.UpdateEntryAsync(entry);

            // Assert
            Assert.True(repo.UpdateCalled);
            Assert.NotNull(entry.LastModified);
            Assert.Equal(8, entry.LastModified.Length);
        }

        [Fact]
        public async Task DeleteEntryAsync_CallsExecuteSqlInterpolatedAsync()
        {
            // Arrange
            var id = Guid.NewGuid();
            var lastModified = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };

            var entryToDelete = new VirusCharacteristicListEntry
            {
                Id = id,
                LastModified = lastModified,
                Name = "Entry to Delete",
                VirusCharacteristicId = Guid.NewGuid()
            };

            var entries = new List<VirusCharacteristicListEntry> { entryToDelete };

            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestVirusCharacteristicListEntryRepository(
                context: mockContext.Object,
                byCharacteristicId: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
                paged: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
                byId: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
                entries: entries.AsQueryable()
            );

            // Act
            await repo.DeleteEntryAsync(id, lastModified);

            // Assert
            Assert.True(repo.DeleteCalled);
        }
        [Fact]
        public async Task GetEntriesByCharacteristicIdAsync_ReturnsCorrectEntries()
        {
            // Arrange
            var characteristicId = Guid.NewGuid();
            var entries = new List<VirusCharacteristicListEntry>
        {
            new VirusCharacteristicListEntry { Id = Guid.NewGuid(), VirusCharacteristicId = characteristicId },
            new VirusCharacteristicListEntry { Id = Guid.NewGuid(), VirusCharacteristicId = characteristicId }
        };
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestVirusCharacteristicListEntryRepository(
                context: mockContext.Object,
                byCharacteristicId: new TestAsyncEnumerable<VirusCharacteristicListEntry>(entries),
                paged: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
              byId: new TestAsyncEnumerable<VirusCharacteristicListEntry>(Enumerable.Empty<VirusCharacteristicListEntry>()),
        entries: entries.AsQueryable()
            );

            // Act
            var result = await repo.GetEntriesByCharacteristicIdAsync(characteristicId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, entry => Assert.Equal(characteristicId, entry.VirusCharacteristicId));
        }


        [Fact]
        public async Task GetVirusCharacteristicListEntries_ReturnsCorrectPage()
        {
            // Arrange
            var characteristicId = Guid.NewGuid();
            var entries = new List<VirusCharacteristicListEntry>
    {
        new VirusCharacteristicListEntry { Id = Guid.NewGuid(), VirusCharacteristicId = characteristicId, Name = "Entry1" },
        new VirusCharacteristicListEntry { Id = Guid.NewGuid(), VirusCharacteristicId = characteristicId, Name = "Entry2" },
        new VirusCharacteristicListEntry { Id = Guid.NewGuid(), VirusCharacteristicId = characteristicId, Name = "Entry3" },
        new VirusCharacteristicListEntry { Id = Guid.NewGuid(), VirusCharacteristicId = characteristicId, Name = "Entry4" },
        new VirusCharacteristicListEntry { Id = Guid.NewGuid(), VirusCharacteristicId = characteristicId, Name = "Entry5" }
    };

            var asyncEntries = new TestAsyncEnumerable<VirusCharacteristicListEntry>(entries);

            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestVirusCharacteristicListEntryRepository(
                context: mockContext.Object,
                byCharacteristicId: asyncEntries,
                paged: asyncEntries,
                byId: asyncEntries,
                entries: asyncEntries
            );

            // Act
            var result = await repo.GetVirusCharacteristicListEntries(characteristicId, pageNo: 2, pageSize: 2);

            // Assert
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal("Entry3", result.Items.First().Name);
            Assert.Equal("Entry4", result.Items.Last().Name);
        }
     
        

    }
}
