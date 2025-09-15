using Apha.VIR.Core.Entities;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.DataAccess.Repositories;
using Apha.VIR.DataAccess.UnitTests.Repository.Helpers;
using Moq;

namespace Apha.VIR.DataAccess.UnitTests.Repository.VirusCharacteristicRepositoryTest
{
    public class TestVirusCharacteristicRepository : VirusCharacteristicRepository
    {
        private readonly IQueryable<VirusCharacteristic> _dataPresent;
        private readonly IQueryable<VirusCharacteristic> _dataAbscent;
        private readonly IQueryable<VirusCharacteristic> _dataAll;
        private readonly IQueryable<VirusCharacteristicDataType> _dataTypeNames;

        public bool AddCalled { get; private set; }
        public bool UpdateCalled { get; private set; }
        public bool DeleteCalled { get; private set; }

        public TestVirusCharacteristicRepository(
            VIRDbContext context,
            IQueryable<VirusCharacteristic> dataAll,
            IQueryable<VirusCharacteristic> dataPresent,
            IQueryable<VirusCharacteristic> dataAbscent,
            IQueryable<VirusCharacteristicDataType>? dataTypeNames = null)
            : base(context)
        {
            _dataAll = dataAll;
            _dataPresent = dataPresent;
            _dataAbscent = dataAbscent;
            _dataTypeNames = dataTypeNames ?? new TestAsyncEnumerable<VirusCharacteristicDataType>(Enumerable.Empty<VirusCharacteristicDataType>());
        }

        protected override IQueryable<T> GetQueryableInterpolatedFor<T>(FormattableString sql)
        {
            if (typeof(T) == typeof(VirusCharacteristic))
            {
                var query = sql.Format.ToLowerInvariant();
                if (query.Contains("getall"))
                    return (IQueryable<T>)_dataAll;
                if (query.Contains("present"))
                    return (IQueryable<T>)_dataPresent;
                if (query.Contains("abscent"))
                    return (IQueryable<T>)_dataAbscent;
            }
            throw new NotImplementedException($"No override for type {typeof(T).Name} and query {sql.Format}");
        }
        protected override IQueryable<T> GetQueryableResultFor<T>(string sql, params object[] parameters)
        {
            if (typeof(T) == typeof(VirusCharacteristicDataType))
                return (IQueryable<T>)_dataTypeNames;
            throw new NotImplementedException($"No override for type {typeof(T).Name} and query {sql}");
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
            return Task.FromResult(0);
        }

        protected override Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
        {
            if (sql.ToLowerInvariant().Contains("delete"))
            {
                DeleteCalled = true;
                return Task.FromResult(1);
            }
            return Task.FromResult(0);
        }
    }
    public class VirusCharacteristicRepositoryTests
    {
        [Fact]
        public async Task GetAllVirusCharacteristicsAsync_ReturnsData()
        {
            var fakeData = new List<VirusCharacteristic>
            {
                new VirusCharacteristic { Id = Guid.NewGuid(), Name = "Char1" },
                new VirusCharacteristic { Id = Guid.NewGuid(), Name = "Char2" }
            };
            var asyncFakeData = new TestAsyncEnumerable<VirusCharacteristic>(fakeData);

            var mockContext = new Mock<VIRDbContext>();

            var repo = new TestVirusCharacteristicRepository(
                context: mockContext.Object,
                dataAll: asyncFakeData,
                dataPresent: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>()),
                dataAbscent: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>())
            );

            var result = await repo.GetAllVirusCharacteristicsAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllVirusCharacteristicsAsync_Paged_ReturnsPagedData()
        {
            var fakeData = new List<VirusCharacteristic>
            {
                new VirusCharacteristic { Id = Guid.NewGuid(), Name = "Char1" },
                new VirusCharacteristic { Id = Guid.NewGuid(), Name = "Char2" },
                new VirusCharacteristic { Id = Guid.NewGuid(), Name = "Char3" }
            };
            var asyncFakeData = new TestAsyncEnumerable<VirusCharacteristic>(fakeData);

            var mockContext = new Mock<VIRDbContext>();

            var repo = new TestVirusCharacteristicRepository(
                context: mockContext.Object,
                dataAll: asyncFakeData,
                dataPresent: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>()),
                dataAbscent: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>())
            );

            var result = await repo.GetAllVirusCharacteristicsAsync(1, 2);

            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(2, result.Items.Count);
        }


        [Fact]
        public async Task GetVirusCharacteristicsByIdAsync_ReturnsCorrectEntry()
        {
            var id = Guid.NewGuid();
            var fakeData = new List<VirusCharacteristic>
            {
                new VirusCharacteristic { Id = id, Name = "Char1" },
                new VirusCharacteristic { Id = Guid.NewGuid(), Name = "Char2" }
            };
            var asyncFakeData = new TestAsyncEnumerable<VirusCharacteristic>(fakeData);

            var mockContext = new Mock<VIRDbContext>();

            var repo = new TestVirusCharacteristicRepository(
                context: mockContext.Object,
                dataAll: asyncFakeData,
                dataPresent: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>()),
                dataAbscent: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>())
            );

            var result = await repo.GetVirusCharacteristicsByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
        }



        [Fact]
        public async Task GetAllVirusCharacteristicsByVirusTypeAsync_AbscentBranch_ReturnsData()
        {
            var virusType = Guid.NewGuid();
            var fakeData = new List<VirusCharacteristic>
    {
        new VirusCharacteristic { Id = virusType, Name = "CharA" }
    };
            var asyncFakeData = new TestAsyncEnumerable<VirusCharacteristic>(fakeData);

            var mockContext = new Mock<VIRDbContext>();

            var repo = new TestVirusCharacteristicRepository(
                context: mockContext.Object,
                dataAll: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>()),
                dataPresent: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>()),
                dataAbscent: asyncFakeData // <-- non-empty for Abscent
            );

            var result = await repo.GetAllVirusCharacteristicsByVirusTypeAsync(virusType, true);

            Assert.NotNull(result);
            Assert.Single(result);
        }


        [Fact]
        public async Task GetAllVirusCharacteristicsByVirusTypeAsync_PresentBranch_ReturnsData()
        {
            var virusType = Guid.NewGuid();
            var fakeData = new List<VirusCharacteristic>
                {
                    new VirusCharacteristic { Id = virusType, Name = "CharB" }
                };
            var asyncFakeData = new TestAsyncEnumerable<VirusCharacteristic>(fakeData);

            var mockContext = new Mock<VIRDbContext>();

            var repo = new TestVirusCharacteristicRepository(
                context: mockContext.Object,
                dataAll: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>()),
                dataPresent: asyncFakeData,
                dataAbscent: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>())
            );

            var result = await repo.GetAllVirusCharacteristicsByVirusTypeAsync(virusType, false);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetAllVirusCharactersticsTypeNamesAsync_ReturnsTypeNames()
        {
            var fakeData = new List<VirusCharacteristicDataType>
            {
                new VirusCharacteristicDataType { Id = Guid.NewGuid(), DataType = "Type1" },
                new VirusCharacteristicDataType { Id = Guid.NewGuid(), DataType = "Type2" }
            };
            var asyncFakeData = new TestAsyncEnumerable<VirusCharacteristicDataType>(fakeData);

            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestVirusCharacteristicRepository(
                context: mockContext.Object,
                dataAll: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>()),
                dataPresent: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>()),
                dataAbscent: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>()),
                dataTypeNames: asyncFakeData
            );

            var result = await repo.GetAllVirusCharactersticsTypeNamesAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task AddEntryAsync_CallsExecuteSqlInterpolatedAsync_AndSetsLastModified()
        {
            var entry = new VirusCharacteristic
            {
                Id = Guid.NewGuid(),
                Name = "Char1"
            };
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestVirusCharacteristicRepository(
                context: mockContext.Object,
                dataAll: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>()),
                dataPresent: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>()),
                dataAbscent: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>())
            );

            await repo.AddEntryAsync(entry);

            Assert.True(repo.AddCalled);
            Assert.NotNull(entry.LastModified);
            Assert.Equal(8, entry.LastModified.Length);
        }

        [Fact]
        public async Task UpdateEntryAsync_CallsExecuteSqlInterpolatedAsync()
        {
            var entry = new VirusCharacteristic
            {
                Id = Guid.NewGuid(),
                Name = "Char1",
                LastModified = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 }
            };
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestVirusCharacteristicRepository(
                context: mockContext.Object,
                dataAll: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>()),
                dataPresent: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>()),
                dataAbscent: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>())
            );

            await repo.UpdateEntryAsync(entry);

            Assert.True(repo.UpdateCalled);
        }

        [Fact]
        public async Task DeleteVirusCharactersticsAsync_CallsExecuteSqlAsync()
        {
            var id = Guid.NewGuid();
            var lastModified = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestVirusCharacteristicRepository(
                context: mockContext.Object,
                dataAll: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>()),
                dataPresent: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>()),
                dataAbscent: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>())
            );

            await repo.DeleteVirusCharactersticsAsync(id, lastModified);

            Assert.True(repo.DeleteCalled);
        }
    }
}
