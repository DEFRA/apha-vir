using Apha.VIR.Core.Entities;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.DataAccess.Repositories;
using Apha.VIR.DataAccess.UnitTests.Repository.Helpers;
using Moq;

namespace Apha.VIR.DataAccess.UnitTests.Repository.IsolateViabilityRepositoryTest
{
    public class TestIsolateViabilityRepository : IsolateViabilityRepository
    {
        private readonly IQueryable<IsolateViability> _viabilityData;

        public bool AddCalled { get; private set; }
        public bool UpdateCalled { get; private set; }
        public bool DeleteCalled { get; private set; }

        public TestIsolateViabilityRepository(VIRDbContext context, IQueryable<IsolateViability> viabilityData)
            : base(context)
        {
            _viabilityData = viabilityData;
        }

        protected override IQueryable<T> GetQueryableResultFor<T>(string sql, params object[] parameters)
        {
            if (typeof(T) == typeof(IsolateViability))
                return (IQueryable<T>)_viabilityData;
            throw new NotImplementedException($"No test data for type {typeof(T).Name}");
        }

        protected override Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
        {
            var lower = sql.ToLowerInvariant();
            if (lower.Contains("insert"))
            {
                AddCalled = true;
                return Task.FromResult(1);
            }
            if (lower.Contains("update"))
            {
                UpdateCalled = true;
                return Task.FromResult(1);
            }
            if (lower.Contains("delete"))
            {
                DeleteCalled = true;
                return Task.FromResult(1);
            }
            throw new NotImplementedException($"No override for query {sql}");
        }
    }

    public class IsolateViabilityRepositoryTests
    {
        [Fact]
        public async Task GetViabilityHistoryAsync_ReturnsData()
        {
            var isolateId = Guid.NewGuid();
            var fakeData = new List<IsolateViability>
            {
                new IsolateViability { IsolateViabilityId = isolateId, IsolateViabilityIsolateId = isolateId }
            };
            var asyncFakeData = new TestAsyncEnumerable<IsolateViability>(fakeData);
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestIsolateViabilityRepository(mockContext.Object, asyncFakeData);

            var result = await repo.GetViabilityHistoryAsync(isolateId);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetViabilityByIsolateIdAsync_ReturnsData()
        {
            var isolateId = Guid.NewGuid();
            var fakeData = new List<IsolateViability>
            {
                new IsolateViability { IsolateViabilityId = isolateId, IsolateViabilityIsolateId = isolateId }
            };
            var asyncFakeData = new TestAsyncEnumerable<IsolateViability>(fakeData);
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestIsolateViabilityRepository(mockContext.Object, asyncFakeData);

            var result = await repo.GetViabilityByIsolateIdAsync(isolateId);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task AddIsolateViabilityAsync_CallsExecuteSqlAsync()
        {
            var isolateViability = new IsolateViability
            {
                IsolateViabilityId = Guid.NewGuid(),
                IsolateViabilityIsolateId = Guid.NewGuid(),
                Viable = Guid.NewGuid(),
                DateChecked = DateTime.UtcNow,
                CheckedById = Guid.NewGuid(),
                LastModified = new byte[8]
            };
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestIsolateViabilityRepository(mockContext.Object, new TestAsyncEnumerable<IsolateViability>(Enumerable.Empty<IsolateViability>()));

            await repo.AddIsolateViabilityAsync(isolateViability, "user1");

            Assert.True(repo.AddCalled);
        }

        [Fact]
        public async Task UpdateIsolateViabilityAsync_CallsExecuteSqlAsync()
        {
            var isolateViability = new IsolateViability
            {
                IsolateViabilityId = Guid.NewGuid(),
                IsolateViabilityIsolateId = Guid.NewGuid(),
                Viable = Guid.NewGuid(),
                DateChecked = DateTime.UtcNow,
                CheckedById = Guid.NewGuid(),
                LastModified = new byte[8]
            };
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestIsolateViabilityRepository(mockContext.Object, new TestAsyncEnumerable<IsolateViability>(Enumerable.Empty<IsolateViability>()));

            await repo.UpdateIsolateViabilityAsync(isolateViability, "user1");

            Assert.True(repo.UpdateCalled);
        }

        [Fact]
        public async Task DeleteIsolateViabilityAsync_CallsExecuteSqlAsync()
        {
            var isolateViabilityId = Guid.NewGuid();
            var lastModified = new byte[8];
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestIsolateViabilityRepository(mockContext.Object, new TestAsyncEnumerable<IsolateViability>(Enumerable.Empty<IsolateViability>()));

            await repo.DeleteIsolateViabilityAsync(isolateViabilityId, lastModified, "user1");

            Assert.True(repo.DeleteCalled);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new IsolateViabilityRepository(null!));
        }
    }
}
