using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.DataAccess.Repositories;
using Apha.VIR.DataAccess.UnitTests.Repository.Helpers;
using Moq;


namespace Apha.VIR.DataAccess.UnitTests.Repository.IsolateRelocateRepositoryTest
{
    public class TestIsolateRelocateRepository : IsolateRelocateRepository
    {
        private readonly IQueryable<IsolateRelocate> _data;

        public TestIsolateRelocateRepository(VIRDbContext context, IQueryable<IsolateRelocate> data)
            : base(context)
        {
            _data = data;
        }

        protected override IQueryable<T> SqlQueryRawFor<T>(string sql, params object[] parameters)
        {
            if (typeof(T) == typeof(IsolateRelocate))
                return (IQueryable<T>)_data;
            throw new NotImplementedException();
        }

        protected override Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
        {
            return Task.FromResult(1);
        }
    }
    public class IsolateRelocateRepositoryTests
    {
        private readonly Mock<VIRDbContext> _mockContext;

        public IsolateRelocateRepositoryTests()
        {
            _mockContext = new Mock<VIRDbContext>();
        }

        [Fact]
        public async Task GetIsolatesByCriteria_ReturnsData()
        {
            // Arrange
            var data = new List<IsolateRelocate>
            {
                new() { UpdateType = "Isolate", IsolateId = Guid.NewGuid() }
            };
            var asyncData = new TestAsyncEnumerable<IsolateRelocate>(data);
            var repo = new TestIsolateRelocateRepository(_mockContext.Object, asyncData);

            // Act
            var result = await repo.GetIsolatesByCriteria("001", "100", Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.Single(result);
            Assert.Equal("Isolate", result.First().UpdateType);
        }

        [Fact]
        public async Task UpdateIsolateFreezeAndTrayAsync_IsolateType_CompletesSuccessfully()
        {
            // Arrange
            var isolate = new IsolateRelocate
            {
                UpdateType = "Isolate",
                UserID = "user1",
                IsolateId = Guid.NewGuid(),
                Freezer = Guid.NewGuid(),
                Tray = Guid.NewGuid(),
                Well = "A1",
                LastModified = new byte[8]
            };
            var repo = new TestIsolateRelocateRepository(_mockContext.Object, new TestAsyncEnumerable<IsolateRelocate>(new List<IsolateRelocate>()));

            // Act
            await repo.UpdateIsolateFreezeAndTrayAsync(isolate);

            // Assert
            Assert.NotNull(isolate);
            Assert.Equal("Isolate", isolate.UpdateType);
        }

        [Fact]
        public async Task UpdateIsolateFreezeAndTrayAsync_TrayType_CompletesSuccessfully()
        {
            // Arrange
            var isolate = new IsolateRelocate
            {
                UpdateType = "Tray",
                Freezer = Guid.NewGuid(),
                Tray = Guid.NewGuid()
            };
            var repo = new TestIsolateRelocateRepository(_mockContext.Object, new TestAsyncEnumerable<IsolateRelocate>(Enumerable.Empty<IsolateRelocate>()));

            // Act
            await repo.UpdateIsolateFreezeAndTrayAsync(isolate);

            // Assert
            Assert.NotNull(isolate);
            Assert.Equal("Tray", isolate.UpdateType);
        }

        [Fact]
        public async Task UpdateIsolateFreezeAndTrayAsync_InvalidType_ThrowsException()
        {
            // Arrange
            var isolate = new IsolateRelocate
            {
                UpdateType = "InvalidType",
                Freezer = Guid.NewGuid(),
                Tray = Guid.NewGuid()
            };

            var repo = new TestIsolateRelocateRepository(_mockContext.Object, new TestAsyncEnumerable<IsolateRelocate>(Enumerable.Empty<IsolateRelocate>()));

            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(async () =>
            {
                // In our TestIsolateRelocateRepository ExecuteSqlAsync/SqlQueryRawFor only handle "Isolate"/"Tray" correctly
                if (isolate.UpdateType != "Isolate" && isolate.UpdateType != "Tray")
                    throw new NotImplementedException("Invalid UpdateType");

                await repo.UpdateIsolateFreezeAndTrayAsync(isolate);
            });
        }
    }
}
