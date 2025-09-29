using Apha.VIR.Core.Entities;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.DataAccess.Repositories;
using Microsoft.Data.SqlClient;
using Moq;

namespace Apha.VIR.DataAccess.UnitTests.Repository.CharacteristicRepositoryTest
{
    public class TestCharacteristicRepository : CharacteristicRepository
    {
        public bool UpdateCalled { get; private set; }
        public FormattableString? LastSql { get; private set; }
        public object[]? LastParams { get; private set; }

        // Delegate for test override
        public Func<Guid, Task<IEnumerable<IsolateCharacteristicInfo>>>? GetIsolateCharacteristicsOverride { get; set; }
        public TestCharacteristicRepository(VIRDbContext context) : base(context) { }

        protected override Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
        {
            UpdateCalled = true;
            LastSql = $"{sql}";
            LastParams = parameters;
            return Task.FromResult(1);
        }

        // Override for testability
        protected override Task<IEnumerable<IsolateCharacteristicInfo>> GetIsolateCharacteristics(Guid isolateId)
        {
            if (GetIsolateCharacteristicsOverride != null)
                return GetIsolateCharacteristicsOverride(isolateId);

            return base.GetIsolateCharacteristics(isolateId);
        }
    }
    public class CharacteristicRepositoryTests
    {
        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CharacteristicRepository(null!));
        }

        [Fact]
        public async Task UpdateIsolateCharacteristicsAsync_CallsExecuteSqlAsync()
        {
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestCharacteristicRepository(mockContext.Object);

            var item = new IsolateCharacteristicInfo
            {
                CharacteristicId = Guid.NewGuid(),
                CharacteristicIsolateId = Guid.NewGuid(),
                VirusCharacteristicId = Guid.NewGuid(),
                CharacteristicValue = "Value",
                LastModified = new byte[8]
            };

            await repo.UpdateIsolateCharacteristicsAsync(item, "user1");

            Assert.True(repo.UpdateCalled);
            Assert.NotNull(repo.LastParams);
            Assert.Contains(repo.LastParams, p => p is SqlParameter sp && sp.ParameterName == "@UserId");
        }

        [Fact]
        public async Task GetIsolateCharacteristicInfoAsync_UsesOverrideAndReturnsTestData()
        {
            // Arrange
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestCharacteristicRepository(mockContext.Object);

            var testId = Guid.NewGuid();
            var expected = new List<IsolateCharacteristicInfo>
        {
            new IsolateCharacteristicInfo
            {
                CharacteristicId = testId,
                CharacteristicIsolateId = Guid.NewGuid(),
                VirusCharacteristicId = Guid.NewGuid(),
                CharacteristicValue = "TestValue",
                LastModified = new byte[8]
            }
        };

            repo.GetIsolateCharacteristicsOverride = id =>
            {
                Assert.Equal(testId, id);
                return Task.FromResult<IEnumerable<IsolateCharacteristicInfo>>(expected);
            };

            // Act
            var result = await repo.GetIsolateCharacteristicInfoAsync(testId);

            // Assert
            Assert.Single(result);
            Assert.Equal("TestValue", result.First().CharacteristicValue);
            Assert.Equal(testId, result.First().CharacteristicId);
        }
    }
}
