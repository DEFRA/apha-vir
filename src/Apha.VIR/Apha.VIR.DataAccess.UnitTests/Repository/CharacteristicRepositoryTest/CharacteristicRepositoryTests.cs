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

        public TestCharacteristicRepository(VIRDbContext context) : base(context) { }

        protected override Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
        {
            UpdateCalled = true;
            LastSql = $"{sql}";
            LastParams = parameters;
            return Task.FromResult(1);
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
    }
}
