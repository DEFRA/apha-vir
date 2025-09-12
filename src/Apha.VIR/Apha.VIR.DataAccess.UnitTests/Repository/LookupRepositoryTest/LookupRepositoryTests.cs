using Apha.VIR.Core.Entities;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.DataAccess.Repositories;
using Apha.VIR.DataAccess.UnitTests.Repository.Helpers;
using Moq;

namespace Apha.VIR.DataAccess.UnitTests.Repository.LookupRepositoryTest
{
    public class TestLookupRepository : LookupRepository
    {
        private readonly IQueryable<LookupItem> _lookupItems;

        public TestLookupRepository(VIRDbContext context, IQueryable<LookupItem> lookupItems)
            : base(context)
        {
            _lookupItems = lookupItems;
        }

        protected override IQueryable<T> GetQueryableResultFor<T>(string sql, params object[] parameters)
        {
            if (typeof(T) == typeof(LookupItem))
                return (IQueryable<T>)_lookupItems;
            throw new NotImplementedException($"No test data for type {typeof(T).Name}");
        }
    }

    public class LookupRepositoryTests
    {
        private static TestLookupRepository CreateRepo(IEnumerable<LookupItem> lookupItems)
        {
            var mockContext = new Mock<VIRDbContext>();
            return new TestLookupRepository(
                mockContext.Object,
                new TestAsyncEnumerable<LookupItem>(lookupItems)
            );
        }

        [Fact]
        public async Task GetAllVirusFamiliesAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllVirusFamiliesAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllVirusTypesAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllVirusTypesAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllHostSpeciesAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllHostSpeciesAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllHostBreedsAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllHostBreedsAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllHostBreedsAltNameAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllHostBreedsAltNameAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllCountriesAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllCountriesAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllHostPurposesAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllHostPurposesAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllSampleTypesAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllSampleTypesAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllWorkGroupsAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllStaffAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllStaffAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllViabilityAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllViabilityAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllSubmittingLabAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllSubmittingLabAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllSubmissionReasonAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllSubmissionReasonAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllIsolationMethodsAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllIsolationMethodsAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllFreezerAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllFreezerAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }

        [Fact]
        public async Task GetAllTraysAsync_ReturnsActiveItems()
        {
            var items = new List<LookupItem>
            {
                new() { Id = Guid.NewGuid(), Active = true },
                new() { Id = Guid.NewGuid(), Active = false }
            };
            var repo = CreateRepo(items);
            var result = await repo.GetAllTraysAsync();
            Assert.Single(result);
            Assert.All(result, x => Assert.True(x.Active));
        }
    }
}
