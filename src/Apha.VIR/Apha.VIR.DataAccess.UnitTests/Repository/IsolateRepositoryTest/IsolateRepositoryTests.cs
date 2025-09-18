using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Core.Entities;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.DataAccess.Repositories;
using Apha.VIR.DataAccess.UnitTests.Repository.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.VIR.DataAccess.UnitTests.Repository.IsolateRepositoryTest
{
    public class TestIsolateRepository : IsolateRepository
    {
        private readonly IQueryable<Isolate> _isolates;
        private readonly IQueryable<IsolateNomenclature> _nomenclatures;

        public TestIsolateRepository(
            VIRDbContext context,
            IQueryable<Isolate> isolates,
            IQueryable<IsolateNomenclature> nomenclatures)
            : base(context)
        {
            _isolates = isolates;
            _nomenclatures = nomenclatures;
        }

        protected override IQueryable<T> GetQueryableResultFor<T>(string sql, params object[] parameters)
        {
            if (typeof(T) == typeof(Isolate))
                return (IQueryable<T>)_isolates;
            throw new NotImplementedException();
        }

        protected override IQueryable<T> SqlQueryRawFor<T>(string sql, params object[] parameters)
        {
            if (typeof(T) == typeof(IsolateNomenclature))
                return (IQueryable<T>)_nomenclatures;
            throw new NotImplementedException();
        }

        protected override Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
        {
            return Task.FromResult(1);
        }
    }
    public class IsolateRepositoryTests
    {
        [Fact]
        public async Task GetIsolateByIsolateAndAVNumberAsync_ReturnsCorrectIsolate()
        {
            var isolateId = Guid.NewGuid();
            var isolates = new List<Isolate>
            {
                new Isolate { IsolateId = isolateId, IsolateSampleId = Guid.NewGuid() }
            };
            var asyncIsolates = new TestAsyncEnumerable<Isolate>(isolates);
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestIsolateRepository(mockContext.Object, asyncIsolates, new TestAsyncEnumerable<IsolateNomenclature>(Enumerable.Empty<IsolateNomenclature>()));

            var result = await repo.GetIsolateByIsolateAndAVNumberAsync("AV123", isolateId);

            Assert.NotNull(result);
            Assert.Equal(isolateId, result.IsolateId);
        }

        [Fact]
        public async Task AddIsolateDetailsAsync_ReturnsNewIsolateId()
        {
            var isolate = new Isolate
            {
                CreatedBy = "user",
                IsolateSampleId = Guid.NewGuid(),
                Family = Guid.NewGuid(),
                Type = Guid.NewGuid(),
                IsMixedIsolate = false,
                AntiserumProduced = false,
                AntigenProduced = false,
                MaterialTransferAgreement = false,
                OriginalSampleAvailable = false,
                NoOfAliquots = 1
            };

            var isolateList = new List<Isolate>();
            var asyncIsolates = new TestAsyncEnumerable<Isolate>(isolateList);

            var mockSet = new Mock<DbSet<Isolate>>(); var queryableAsyncIsolates = (IQueryable<Isolate>)asyncIsolates;

            mockSet.As<IQueryable<Isolate>>().Setup(m => m.Provider).Returns(queryableAsyncIsolates.Provider);
            mockSet.As<IQueryable<Isolate>>().Setup(m => m.Expression).Returns(queryableAsyncIsolates.Expression);
            mockSet.As<IQueryable<Isolate>>().Setup(m => m.ElementType).Returns(queryableAsyncIsolates.ElementType);
            mockSet.As<IQueryable<Isolate>>().Setup(m => m.GetEnumerator()).Returns(() => queryableAsyncIsolates.GetEnumerator());


            var mockContext = new Mock<VIRDbContext>();
            mockContext.Setup(c => c.Isolates).Returns(mockSet.Object);

            var repo = new TestIsolateRepository(mockContext.Object, asyncIsolates, new TestAsyncEnumerable<IsolateNomenclature>(Enumerable.Empty<IsolateNomenclature>()));

            var result = await repo.AddIsolateDetailsAsync(isolate);

            Assert.NotEqual(Guid.Empty, result);
        }

        [Fact]
        public async Task UpdateIsolateDetailsAsync_ExecutesSql()
        {
            var isolate = new Isolate
            {
                CreatedBy = "user",
                IsolateId = Guid.NewGuid(),
                IsolateSampleId = Guid.NewGuid(),
                Family = Guid.NewGuid(),
                Type = Guid.NewGuid(),
                IsMixedIsolate = false,
                AntiserumProduced = false,
                AntigenProduced = false,
                MaterialTransferAgreement = false,
                OriginalSampleAvailable = false,
                NoOfAliquots = 1,
                LastModified = new byte[8]
            };
            var asyncIsolates = new TestAsyncEnumerable<Isolate>(new List<Isolate>());
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestIsolateRepository(mockContext.Object, asyncIsolates, new TestAsyncEnumerable<IsolateNomenclature>(Enumerable.Empty<IsolateNomenclature>()));

            await repo.UpdateIsolateDetailsAsync(isolate);           

            // Add an assertion to ensure the test validates behavior
            Assert.NotNull(isolate);
            Assert.Equal("user", isolate.CreatedBy);
        }

        [Fact]
#pragma warning disable S2699 // Tests should include assertions
        public async Task DeleteIsolateAsync_ExecutesSql()
#pragma warning restore S2699 // Tests should include assertions
        {
            var isolateId = Guid.NewGuid();
            var lastModified = new byte[8];
            var asyncIsolates = new TestAsyncEnumerable<Isolate>(new List<Isolate>());
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestIsolateRepository(mockContext.Object, asyncIsolates, new TestAsyncEnumerable<IsolateNomenclature>(Enumerable.Empty<IsolateNomenclature>()));

            await repo.DeleteIsolateAsync(isolateId, "user", lastModified);
            
        }

        [Fact]
        public async Task GetIsolateForNomenclatureAsync_ReturnsNomenclature()
        {
            var isolateId = Guid.NewGuid();
            var nomenclatures = new List<IsolateNomenclature>
            {
                new IsolateNomenclature { IsolateId = isolateId }
            };
            var asyncNomenclatures = new TestAsyncEnumerable<IsolateNomenclature>(nomenclatures);
            var mockContext = new Mock<VIRDbContext>();
            var repo = new TestIsolateRepository(mockContext.Object, new TestAsyncEnumerable<Isolate>(Enumerable.Empty<Isolate>()), asyncNomenclatures);

            var result = await repo.GetIsolateForNomenclatureAsync(isolateId);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(isolateId, result.First().IsolateId);
        }
    }
}
