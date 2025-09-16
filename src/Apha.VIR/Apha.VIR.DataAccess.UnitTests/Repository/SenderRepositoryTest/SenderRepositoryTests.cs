using Apha.VIR.Core.Entities;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.DataAccess.Repositories;
using Apha.VIR.DataAccess.UnitTests.Repository.Helpers;
using Moq;

namespace Apha.VIR.DataAccess.UnitTests.Repository.SenderRepositoryTest
{
    public class TestSenderRepository : SenderRepository
    {
        private readonly IEnumerable<Sender> _senders;

        public bool AddCalled { get; private set; }
        public bool UpdateCalled { get; private set; }
        public bool DeleteCalled { get; private set; }

        public TestSenderRepository(VIRDbContext context, IEnumerable<Sender> senders) : base(context)
        {
            _senders = senders;
        }

        protected override IQueryable<T> GetQueryableResultFor<T>(string sql, params object[] parameters)
        {
            if (typeof(T) == typeof(Sender))
                return (IQueryable<T>)new TestAsyncEnumerable<Sender>(_senders)!; // Suppress nullability warning with `!`
            throw new NotImplementedException();
        }

        protected override IQueryable<T> GetQueryableInterpolatedFor<T>(FormattableString sql)
        {
            if (typeof(T) == typeof(Sender))
                return (IQueryable<T>)new TestAsyncEnumerable<Sender>(_senders)!; // Suppress nullability warning with `!`
            throw new NotImplementedException();
        }

        protected override Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
        {
            if (sql.Contains("Insert"))
            {
                AddCalled = true;
                return Task.FromResult(1);
            }
            if (sql.Contains("Update"))
            {
                UpdateCalled = true;
                return Task.FromResult(1);
            }
            if (sql.Contains("Delete"))
            {
                DeleteCalled = true;
                return Task.FromResult(1);
            }
            return Task.FromResult(0);
        }
    }
    public class SenderRepositoryTests
    {
        [Fact]
        public async Task GetAllSenderOrderBySenderAsync_FiltersByCountryId()
        {
            var countryId = Guid.NewGuid();
            var senders = new List<Sender>
            {
                new Sender { SenderId = Guid.NewGuid(), Country = countryId },
                new Sender { SenderId = Guid.NewGuid(), Country = Guid.NewGuid() }
            };
            var repo = new TestSenderRepository(new Mock<VIRDbContext>().Object, senders);
            var result = await repo.GetAllSenderOrderBySenderAsync(countryId);
            Assert.Single(result);
            Assert.Equal(countryId, result.First().Country);
        }

        [Fact]
        public async Task GetAllSenderOrderByOrganisationAsync_FiltersByCountryId()
        {
            var countryId = Guid.NewGuid();
            var senders = new List<Sender>
            {
                new Sender { SenderId = Guid.NewGuid(), Country = countryId },
                new Sender { SenderId = Guid.NewGuid(), Country = Guid.NewGuid() }
            };
            var repo = new TestSenderRepository(new Mock<VIRDbContext>().Object, senders);
            var result = await repo.GetAllSenderOrderByOrganisationAsync(countryId);
            Assert.Single(result);
            Assert.Equal(countryId, result.First().Country);
        }

        [Fact]
        public async Task GetAllSenderAsync_ReturnsPagedData()
        {
            var senders = new List<Sender>
            {
                new Sender { SenderId = Guid.NewGuid() },
                new Sender { SenderId = Guid.NewGuid() },
                new Sender { SenderId = Guid.NewGuid() }
            };
            var repo = new TestSenderRepository(new Mock<VIRDbContext>().Object, senders);
            var result = await repo.GetAllSenderAsync(1, 2);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(2, result.Items.Count);
        }

        [Fact]
        public async Task GetSenderAsync_ReturnsSenderOrDefault()
        {
            var senderId = Guid.NewGuid();
            var senders = new List<Sender>
            {
                new Sender { SenderId = senderId }
            };
            var repo = new TestSenderRepository(new Mock<VIRDbContext>().Object, senders);
            var result = await repo.GetSenderAsync(senderId);
            Assert.NotNull(result);
            Assert.Equal(senderId, result.SenderId);

            // Test for not found
            var repo2 = new TestSenderRepository(new Mock<VIRDbContext>().Object, new List<Sender>());
            var result2 = await repo2.GetSenderAsync(Guid.NewGuid());
            Assert.NotNull(result2);
        }

        [Fact]
        public async Task AddSenderAsync_CallsExecuteSqlRawAsync()
        {
            var repo = new TestSenderRepository(new Mock<VIRDbContext>().Object, new List<Sender>());
            await repo.AddSenderAsync(new Sender());
            Assert.True(repo.AddCalled);
        }

        [Fact]
        public async Task UpdateSenderAsync_CallsExecuteSqlRawAsync()
        {
            var repo = new TestSenderRepository(new Mock<VIRDbContext>().Object, new List<Sender>());
            await repo.UpdateSenderAsync(new Sender());
            Assert.True(repo.UpdateCalled);
        }

        [Fact]
        public async Task DeleteSenderAsync_CallsExecuteSqlRawAsync()
        {
            var repo = new TestSenderRepository(new Mock<VIRDbContext>().Object, new List<Sender>());
            await repo.DeleteSenderAsync(Guid.NewGuid());
            Assert.True(repo.DeleteCalled);
        }
    }
}
