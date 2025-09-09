using Apha.VIR.Core.Entities;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.DataAccess.Repositories;
using Apha.VIR.DataAccess.UnitTests.Repository.Helpers;
using Moq;

namespace Apha.VIR.DataAccess.UnitTests.Repository.SubmissionRepositoryTest
{
    public class TestSubmissionRepository : SubmissionRepository
    {
        public bool InsertCalled { get; private set; }
        public bool UpdateCalled { get; private set; }

        private readonly IQueryable<int> _fakeCounts;
        private readonly IQueryable<string> _fakeStrings;

        public TestSubmissionRepository(
            VIRDbContext context,
            IQueryable<int> fakeCounts,
            IQueryable<string> fakeStrings) : base(context)
        {
            _fakeCounts = fakeCounts;
            _fakeStrings = fakeStrings;
        }

        protected override IQueryable<T> SqlQueryInterpolatedFor<T>(FormattableString sql)
        {
            if (typeof(T) == typeof(int))
                return (IQueryable<T>)_fakeCounts;
            if (typeof(T) == typeof(string))
                return (IQueryable<T>)_fakeStrings;

            throw new NotImplementedException($"No fake defined for type {typeof(T).Name}");
        }

        protected override Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
        {
            var query = sql.ToLowerInvariant();

            if (query.Contains("insert"))
            {
                InsertCalled = true;
                return Task.FromResult(1);
            }

            if (query.Contains("update"))
            {
                UpdateCalled = true;
                return Task.FromResult(1);
            }

            throw new NotImplementedException($"No override for query {sql}");
        }
    }
    public class SubmissionRepositoryTests
    {
        [Fact]
        public async Task AVNumberExistsInVirAsync_ReturnsTrue_WhenCountGreaterThanZero()
        {
            var fakeCounts = new TestAsyncEnumerable<int>(new[] { 1 });
            var repo = new TestSubmissionRepository(new Mock<VIRDbContext>().Object, fakeCounts, new TestAsyncEnumerable<string>(Array.Empty<string>()));

            var result = await repo.AVNumberExistsInVirAsync("AV123");

            Assert.True(result);
        }

        [Fact]
        public async Task AVNumberExistsInVirAsync_ReturnsFalse_WhenCountZero()
        {
            var fakeCounts = new TestAsyncEnumerable<int>(new[] { 0 });
            var repo = new TestSubmissionRepository(new Mock<VIRDbContext>().Object, fakeCounts, new TestAsyncEnumerable<string>(Array.Empty<string>()));

            var result = await repo.AVNumberExistsInVirAsync("AV123");

            Assert.False(result);
        }

        [Fact]
        public async Task GetLatestSubmissionsAsync_ReturnsStrings()
        {
            var fakeStrings = new TestAsyncEnumerable<string>(new[] { "AV001", "AV002" });
            var repo = new TestSubmissionRepository(new Mock<VIRDbContext>().Object, new TestAsyncEnumerable<int>(Array.Empty<int>()), fakeStrings);

            var result = await repo.GetLatestSubmissionsAsync();

            Assert.Equal(2, result.Count());
            Assert.Contains("AV001", result);
            Assert.Contains("AV002", result);
        }

        [Fact]
        public async Task AddSubmissionAsync_CallsExecuteSqlAsync()
        {
            var repo = new TestSubmissionRepository(new Mock<VIRDbContext>().Object, new TestAsyncEnumerable<int>(Array.Empty<int>()), new TestAsyncEnumerable<string>(Array.Empty<string>()));
            var submission = new Submission { Avnumber = "AV123" };

            await repo.AddSubmissionAsync(submission, "user");

            Assert.True(repo.InsertCalled);
        }

        [Fact]
        public async Task UpdateSubmissionAsync_CallsExecuteSqlAsync()
        {
            var repo = new TestSubmissionRepository(new Mock<VIRDbContext>().Object, new TestAsyncEnumerable<int>(Array.Empty<int>()), new TestAsyncEnumerable<string>(Array.Empty<string>()));
            var submission = new Submission { SubmissionId = Guid.NewGuid(), Avnumber = "AV123", LastModified = new byte[8] };

            await repo.UpdateSubmissionAsync(submission, "user");

            Assert.True(repo.UpdateCalled);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SubmissionRepository(null!));
        }
    }
}
