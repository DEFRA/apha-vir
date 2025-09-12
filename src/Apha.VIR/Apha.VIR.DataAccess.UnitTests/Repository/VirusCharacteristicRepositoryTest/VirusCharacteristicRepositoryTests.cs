using Apha.VIR.Core.Entities;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.DataAccess.Repositories;
using Apha.VIR.DataAccess.UnitTests.Repository.Helpers;
using Moq;

namespace Apha.VIR.DataAccess.UnitTests.Repository.VirusCharacteristicRepositoryTest
{
    public class TestVirusCharacteristicRepository : VirusCharacteristicRepository
    {
        private readonly IQueryable<VirusCharacteristic> _dataPresent;
        private readonly IQueryable<VirusCharacteristic> _dataAbscent;
        private readonly IQueryable<VirusCharacteristic> _dataAll;

        public TestVirusCharacteristicRepository(
            VIRDbContext context,
            IQueryable<VirusCharacteristic> dataAll,
            IQueryable<VirusCharacteristic> dataPresent,
            IQueryable<VirusCharacteristic> dataAbscent)
            : base(context)
        {
            _dataAll = dataAll;
            _dataPresent = dataPresent;
            _dataAbscent = dataAbscent;
        }

        protected override IQueryable<T> GetQueryableInterpolatedFor<T>(FormattableString sql)
        {
            if (typeof(T) == typeof(VirusCharacteristic))
            {
                var query = sql.Format.ToLowerInvariant();
                if (query.Contains("getall"))
                    return (IQueryable<T>)_dataAll;
                if (query.Contains("present"))
                    return (IQueryable<T>)_dataPresent;
                if (query.Contains("abscent"))
                    return (IQueryable<T>)_dataAbscent;
            }
            throw new NotImplementedException($"No override for type {typeof(T).Name} and query {sql.Format}");
        }
    }
    public class VirusCharacteristicRepositoryTests
    {
        [Fact]
        public async Task GetAllVirusCharacteristicsAsync_ReturnsData()
        {
            var fakeData = new List<VirusCharacteristic>
            {
                new VirusCharacteristic { Id = Guid.NewGuid(), Name = "Char1" },
                new VirusCharacteristic { Id = Guid.NewGuid(), Name = "Char2" }
            };
            var asyncFakeData = new TestAsyncEnumerable<VirusCharacteristic>(fakeData);

            var mockContext = new Mock<VIRDbContext>();

            var repo = new TestVirusCharacteristicRepository(
                context: mockContext.Object,
                dataAll: asyncFakeData,
                dataPresent: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>()),
                dataAbscent: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>())
            );

            var result = await repo.GetAllVirusCharacteristicsAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }


        [Fact]
        public async Task GetAllVirusCharacteristicsByVirusTypeAsync_AbscentBranch_ReturnsData()
        {
            var virusType = Guid.NewGuid();
            var fakeData = new List<VirusCharacteristic>
    {
        new VirusCharacteristic { Id = virusType, Name = "CharA" }
    };
            var asyncFakeData = new TestAsyncEnumerable<VirusCharacteristic>(fakeData);

            var mockContext = new Mock<VIRDbContext>();

            var repo = new TestVirusCharacteristicRepository(
                context: mockContext.Object,
                dataAll: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>()),
                dataPresent: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>()),
                dataAbscent: asyncFakeData // <-- non-empty for Abscent
            );

            var result = await repo.GetAllVirusCharacteristicsByVirusTypeAsync(virusType, true);

            Assert.NotNull(result);
            Assert.Single(result);
        }


        [Fact]
        public async Task GetAllVirusCharacteristicsByVirusTypeAsync_PresentBranch_ReturnsData()
        {
            var virusType = Guid.NewGuid();
            var fakeData = new List<VirusCharacteristic>
                {
                    new VirusCharacteristic { Id = virusType, Name = "CharB" }
                };
            var asyncFakeData = new TestAsyncEnumerable<VirusCharacteristic>(fakeData);

            var mockContext = new Mock<VIRDbContext>();

            var repo = new TestVirusCharacteristicRepository(
                context: mockContext.Object,
                dataAll: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>()),
                dataPresent: asyncFakeData,
                dataAbscent: new TestAsyncEnumerable<VirusCharacteristic>(Enumerable.Empty<VirusCharacteristic>())
            );

            var result = await repo.GetAllVirusCharacteristicsByVirusTypeAsync(virusType, false);

            Assert.NotNull(result);
            Assert.Single(result);
        }
    }
}
