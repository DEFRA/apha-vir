using Apha.VIR.Core.Entities;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.DataAccess.Repositories;
using Moq;

namespace Apha.VIR.DataAccess.UnitTests.Repository.DispatchRepositoryTest
{
    public class TestDispatchRepository : DispatchRepository
    {
        public bool AddCalled { get; private set; }
        public bool UpdateCalled { get; private set; }
        public bool DeleteCalled { get; private set; }

        public TestDispatchRepository(VIRDbContext context) : base(context) { }

        protected override Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
        {
            if (sql.Contains("spDispatchInsert", StringComparison.OrdinalIgnoreCase))
            {
                AddCalled = true;
                return Task.FromResult(1);
            }
            if (sql.Contains("spDispatchUpdate", StringComparison.OrdinalIgnoreCase))
            {
                UpdateCalled = true;
                return Task.FromResult(1);
            }
            if (sql.Contains("spDispatchDelete", StringComparison.OrdinalIgnoreCase))
            {
                DeleteCalled = true;
                return Task.FromResult(1);
            }
            throw new NotImplementedException($"No override for query {sql}");
        }
    }

    public class DispatchRepositoryTests
    {
        [Fact]
        public async Task AddDispatchAsync_CallsExecuteSqlAsync()
        {
            var dispatchInfo = new IsolateDispatchInfo
            {
                DispatchIsolateId = Guid.NewGuid(),
                NoOfAliquots = 5,
                RecipientId = Guid.NewGuid(),
                RecipientName = "Recipient",
                RecipientAddress = "Address",
                ReasonForDispatch = "Reason",
                DispatchedDate = DateTime.UtcNow,
                DispatchedById = Guid.NewGuid()
            };
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestDispatchRepository(mockContext.Object);

            await repo.AddDispatchAsync(dispatchInfo, "user1");

            Assert.True(repo.AddCalled);
        }

        [Fact]
        public async Task UpdateDispatchAsync_CallsExecuteSqlAsync()
        {
            var dispatchInfo = new IsolateDispatchInfo
            {
                DispatchId = Guid.NewGuid(),
                DispatchIsolateId = Guid.NewGuid(),
                NoOfAliquots = 5,
                RecipientId = Guid.NewGuid(),
                RecipientName = "Recipient",
                RecipientAddress = "Address",
                ReasonForDispatch = "Reason",
                DispatchedDate = DateTime.UtcNow,
                DispatchedById = Guid.NewGuid()
            };
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestDispatchRepository(mockContext.Object);

            await repo.UpdateDispatchAsync(dispatchInfo, "user2");

            Assert.True(repo.UpdateCalled);
        }

        [Fact]
        public async Task DeleteDispatchAsync_CallsExecuteSqlAsync()
        {
            var dispatchId = Guid.NewGuid();
            var lastModified = new byte[8];
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestDispatchRepository(mockContext.Object);

            await repo.DeleteDispatchAsync(dispatchId, lastModified, "user3");

            Assert.True(repo.DeleteCalled);
        }
    }
}
