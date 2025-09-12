using Apha.VIR.Core.Entities;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.DataAccess.Repositories;
using Apha.VIR.DataAccess.UnitTests.Repository.Helpers;
using Moq;

namespace Apha.VIR.DataAccess.UnitTests.Repository.StaffRepositoryTest
{
    public class TestStaffRepository : StaffRepository
    {
        private readonly IQueryable<Staff> _staffData;

        public TestStaffRepository(VIRDbContext context, IQueryable<Staff> staffData)
            : base(context)
        {
            _staffData = staffData;
        }

        protected override IQueryable<T> GetDbSetFor<T>()
        {
            if (typeof(T) == typeof(Staff))
                return (IQueryable<T>)_staffData;
            throw new NotImplementedException($"No test data for type {typeof(T).Name}");
        }
    }
    public class StaffRepositoryTests
    {
        [Fact]
        public async Task GetStaffListAsync_ReturnsStaffList()
        {
            // Arrange
            var staffList = new List<Staff>
            {
                new Staff { Name = "Alice" },
                new Staff { Name = "Bob" }
            };
            var asyncStaffData = new TestAsyncEnumerable<Staff>(staffList);
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestStaffRepository(mockContext.Object, asyncStaffData);

            // Act
            var result = await repo.GetStaffListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, s => s.Name == "Alice");
            Assert.Contains(result, s => s.Name == "Bob");
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new StaffRepository(null!));
        }
    }
}
