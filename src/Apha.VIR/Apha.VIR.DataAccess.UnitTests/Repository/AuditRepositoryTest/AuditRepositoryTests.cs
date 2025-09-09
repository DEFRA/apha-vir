using System.Globalization;
using Apha.VIR.Core.Entities;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.DataAccess.Repositories;
using Apha.VIR.DataAccess.UnitTests.Repository.Helpers;
using Microsoft.Data.SqlClient;
using Moq;

namespace Apha.VIR.DataAccess.UnitTests.Repository.AuditRepositoryTest
{
    public class TestAuditRepository : AuditRepository
    {
        private readonly Dictionary<Type, IQueryable> _data;

        public TestAuditRepository(VIRDbContext context, Dictionary<Type, IQueryable> data)
            : base(context)
        {
            _data = data;
        }

        protected override IQueryable<T> GetQueryableResultFor<T>(string sql, params object[] parameters)
        {
            if (_data.TryGetValue(typeof(T), out var queryable))
                return (IQueryable<T>)queryable;
            throw new NotImplementedException($"No test data for type {typeof(T).Name}");
        }
    }
    public class AuditRepositoryTests
    {
        private static TestAuditRepository CreateRepo<T>(IEnumerable<T> data) where T : class
        {
            var mockContext = new Mock<VIRDbContext>();
            var dict = new Dictionary<Type, IQueryable>
    {
        { typeof(T), new TestAsyncEnumerable<T>(data) }
    };
            return new TestAuditRepository(mockContext.Object, dict);
        }


        [Fact]
        public async Task GetSubmissionLogsAsync_ReturnsData()
        {
            var logs = new List<AuditSubmissionLog>
            {
                new AuditSubmissionLog { LogID = Guid.NewGuid(), UserId = "user1", AVNumber = "AV1" }
            };
            var repo = CreateRepo(logs);
            var result = await repo.GetSubmissionLogsAsync("AV1", DateTime.Now, DateTime.Now, "user1");
            Assert.Single(result);
        }

        [Fact]
        public async Task GetCharacteristicsLogsAsync_ReturnsData()
        {
            var logs = new List<AuditCharacteristicLog>
            {
                new AuditCharacteristicLog { LogId = Guid.NewGuid(), UserId = "user2", CharacteristicId = Guid.NewGuid() }
            };
            var repo = CreateRepo(logs);
            var result = await repo.GetCharacteristicsLogsAsync("AV2", null, null, "user2");
            Assert.Single(result);
        }

        [Fact]
        public async Task GetDispatchLogsAsync_ReturnsData()
        {
            var logs = new List<AuditDispatchLog>
            {
                new AuditDispatchLog { LogId = Guid.NewGuid(), UserId = "user3", DispatchIsolateId = Guid.NewGuid() }
            };
            var repo = CreateRepo(logs);
            var result = await repo.GetDispatchLogsAsync("AV3", null, null, "user3");
            Assert.Single(result);
        }

        [Fact]
        public async Task GetIsolateViabilityLogsAsync_ReturnsData()
        {
            var logs = new List<AuditViabilityLog>
            {
                new AuditViabilityLog { LogId = Guid.NewGuid(), UserId = "user4", IsolateViabilityId = Guid.NewGuid() }
            };
            var repo = CreateRepo(logs);
            var result = await repo.GetIsolateViabilityLogsAsync("AV4", null, null, "user4");
            Assert.Single(result);
        }

        [Fact]
        public async Task GetIsolatLogsAsync_ReturnsData()
        {
            var logs = new List<AuditIsolateLog>
            {
                new AuditIsolateLog { LogId = Guid.NewGuid(), UserId = "user5", IsolateSampleId = Guid.NewGuid() }
            };
            var repo = CreateRepo(logs);
            var result = await repo.GetIsolatLogsAsync("AV5", null, null, "user5");
            Assert.Single(result);
        }

        [Fact]
        public async Task GetSamplLogsAsync_ReturnsData()
        {
            var logs = new List<AuditSampleLog>
            {
                new AuditSampleLog { LogId = Guid.NewGuid(), UserId = "user6", SampleSubmissionId = Guid.NewGuid() }
            };
            var repo = CreateRepo(logs);
            var result = await repo.GetSamplLogsAsync("AV6", null, null, "user6");
            Assert.Single(result);
        }

        [Fact]
        public async Task GetIsolatLogDetailAsync_ReturnsData()
        {
            var logs = new List<AuditIsolateLogDetail>
            {
                new AuditIsolateLogDetail { LogId = Guid.NewGuid(), UserId = "user7" }
            };
            var repo = CreateRepo(logs);
            var result = await repo.GetIsolatLogDetailAsync(Guid.NewGuid());
            Assert.Single(result);
        }

        [Theory]
        [InlineData(null, null, null, null)]
        [InlineData("AV", null, null, null)]
        [InlineData(null, "2024-01-01", null, null)]
        [InlineData(null, null, "2024-01-01", null)]
        [InlineData(null, null, null, "user")]
        public void GetSqlParameters_HandlesNulls(string avNumber, string dateFromStr, string dateToStr, string userid)
        {
            DateTime? dateFrom = dateFromStr != null
                ? DateTime.Parse(dateFromStr, CultureInfo.InvariantCulture)
                : (DateTime?)null;
            DateTime? dateTo = dateToStr != null
                ? DateTime.Parse(dateToStr, CultureInfo.InvariantCulture)
                : (DateTime?)null;

            var methodInfo = typeof(AuditRepository)
                .GetMethod("GetSqlParameters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            Assert.NotNull(methodInfo);

            var parametersObj = methodInfo.Invoke(null, new object[] { avNumber, dateFrom, dateTo, userid });
            Assert.NotNull(parametersObj);

            var parameters = parametersObj as SqlParameter[];
            Assert.NotNull(parameters);

            Assert.Equal(4, parameters.Length);
            Assert.All(parameters, p => Assert.NotNull(p));
        }

    }
}
