using Apha.VIR.Core.Entities;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.DataAccess.Repositories;
using Apha.VIR.DataAccess.UnitTests.Repository.Helpers;
using Moq;

namespace Apha.VIR.DataAccess.UnitTests.Repository.WorkgroupRepositoryTest
{
    public class TestWorkgroupRepository : WorkgroupRepository
    {
        private readonly IQueryable<Workgroup> _workgroupData;

        public TestWorkgroupRepository(VIRDbContext context, IQueryable<Workgroup> workgroupData)
            : base(context)
        {
            _workgroupData = workgroupData;
        }

        protected override IQueryable<T> GetDbSetFor<T>()
        {
            if (typeof(T) == typeof(Workgroup))
                return (IQueryable<T>)_workgroupData;
            throw new NotImplementedException($"No test data for type {typeof(T).Name}");
        }
    }
    public class WorkgroupRepositoryTests
    {
        [Fact]
        public async Task GetWorkgroupfListAsync_ReturnsWorkgroupList()
        {
            // Arrange
            var workgroupList = new List<Workgroup>
            {
                new Workgroup { Name = "WG1" },
                new Workgroup { Name = "WG2" }
            };
            var asyncWorkgroupData = new TestAsyncEnumerable<Workgroup>(workgroupList);
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestWorkgroupRepository(mockContext.Object, asyncWorkgroupData);

            // Act
            var result = await repo.GetWorkgroupfListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, w => w.Name == "WG1");
            Assert.Contains(result, w => w.Name == "WG2");
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkgroupRepository(null!));
        }
    }
}
